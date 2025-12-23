using ApplicationCore.Interfaces; // Добавьте этот using для ITransactionService
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.Service;
using LC.BLL.Session;
using LepreCoins.Models; // Проверьте, что модель Budget находится здесь
using System.Collections.ObjectModel;

namespace LepreCoinsApp.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    private readonly IBudgetService _budgetService;
    private readonly ITransactionService _transactionService; // 1. Добавили сервис транзакций

    // 2. Добавили свойство IsBusy для индикации загрузки
    [ObservableProperty]
    private bool _isBusy;

    public BudgetViewModel(IBudgetService budgetService, ITransactionService transactionService)
    {
        _budgetService = budgetService;
        _transactionService = transactionService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsRemaining), nameof(WantsRemaining), nameof(SavingsRemaining))]
    [NotifyPropertyChangedFor(nameof(NeedsProgress), nameof(WantsProgress), nameof(SavingsProgress))]
    [NotifyPropertyChangedFor(nameof(TotalProgress))]
    private Budget _currentBudget;

    [ObservableProperty]
    private bool _hasBudget;

    [ObservableProperty]
    private bool _hasNoBudget;

    // --- РАСЧЕТ ОСТАТКОВ В РУБЛЯХ ---

    public decimal NeedsRemaining =>
        CalculateRemaining(CurrentBudget?.EstablishedAmount, CurrentBudget?.NeedsPercentage, CurrentBudget?.SpentNeeds);

    public decimal WantsRemaining =>
        CalculateRemaining(CurrentBudget?.EstablishedAmount, CurrentBudget?.WantsPercentage, CurrentBudget?.SpentWants);

    public decimal SavingsRemaining =>
        CalculateRemaining(CurrentBudget?.EstablishedAmount, CurrentBudget?.SavingsPercentage, CurrentBudget?.SpentSavings);


    // --- РАСЧЕТ ПРОГРЕССА (для ProgressBar) ---

    public double NeedsProgress =>
        CalculateProgress(CurrentBudget?.EstablishedAmount, CurrentBudget?.NeedsPercentage, CurrentBudget?.SpentNeeds);

    public double WantsProgress =>
        CalculateProgress(CurrentBudget?.EstablishedAmount, CurrentBudget?.WantsPercentage, CurrentBudget?.SpentWants);

    public double SavingsProgress =>
        CalculateProgress(CurrentBudget?.EstablishedAmount, CurrentBudget?.SavingsPercentage, CurrentBudget?.SpentSavings);

    public double TotalProgress
    {
        get
        {
            if (CurrentBudget?.EstablishedAmount == null || CurrentBudget.EstablishedAmount <= 0) return 0;
            double progress = (double)((CurrentBudget.CurrentExpenses ?? 0) / CurrentBudget.EstablishedAmount.Value);
            return progress > 1 ? 1 : progress;
        }
    }

    // --- КОМАНДЫ ---

    [RelayCommand]
    public async Task LoadBudgetData()
    {
        var budgetId = Session.CurrentUser?.Budgetid;

        if (budgetId != null && budgetId > 0)
        {
            // 1. Получаем свежие данные из сервиса без кэша
            var budget = await _budgetService.GetBudgetByIdAsync(budgetId.Value);

            if (budget != null)
            {
                // 2. Присваиваем новый объект. 
                // Благодаря [NotifyPropertyChangedFor] в заголовке класса, 
                // это автоматически дернет обновление NeedsProgress, TotalProgress и т.д.
                CurrentBudget = budget;

                HasBudget = true;
                HasNoBudget = false;

                // 3. На всякий случай принудительно уведомляем UI о пересчете
                OnPropertyChanged(nameof(NeedsRemaining));
                OnPropertyChanged(nameof(WantsRemaining));
                OnPropertyChanged(nameof(SavingsRemaining));
                OnPropertyChanged(nameof(NeedsProgress));
                OnPropertyChanged(nameof(WantsProgress));
                OnPropertyChanged(nameof(SavingsProgress));
                OnPropertyChanged(nameof(TotalProgress));
                return;
            }
        }

        HasBudget = false;
        HasNoBudget = true;
    }

    [RelayCommand]
    public async Task ResetPeriodAsync()
    {
        if (Session.CurrentUser == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Перезапуск периода",
            "Счетчики будут обнулены. Продолжить?",
            "Да", "Отмена");

        if (!confirm) return;

        IsBusy = true;
        var result = await _budgetService.ResetBudgetPeriodAsync(Session.CurrentUser.Id);

        if (result.IsSuccess)
        {
            // ВАЖНО: Сначала загружаем данные, потом выключаем IsBusy
            await LoadBudgetData();
            IsBusy = false;

            // Короткое уведомление (по желанию)
            // await Shell.Current.DisplayAlert("Успех", "Данные обновлены", "OK");
        }
        else
        {
            IsBusy = false;
            await Shell.Current.DisplayAlert("Ошибка", result.ErrorMessage, "OK");
        }
    }

    [RelayCommand]
    private async Task CreateBudget()
    {
        await Shell.Current.GoToAsync("BudgetCreatePage");
    }


    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    private decimal CalculateRemaining(decimal? total, int? percent, decimal? spent)
    {
        if (total == null || percent == null) return 0;
        decimal limit = total.Value * percent.Value / 100;
        return limit - (spent ?? 0);
    }

    private double CalculateProgress(decimal? total, int? percent, decimal? spent)
    {
        if (total == null || percent == null || percent <= 0 || spent == null) return 0;

        decimal limit = total.Value * percent.Value / 100;
        if (limit <= 0) return 0;

        double progress = (double)(spent.Value / limit);
        return progress > 1 ? 1 : progress;
    }
}