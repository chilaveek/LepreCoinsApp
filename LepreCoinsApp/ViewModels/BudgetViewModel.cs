using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;
using LC.BLL.Session;
using LepreCoins.Models;
using LepreCoinsApp;
using LepreCoinsApp.ViewModels;

namespace LepreCoinsApp.Views;

public partial class BudgetViewModel : ObservableObject
{
    private readonly IBudgetService _budgetService;
    [NotifyPropertyChangedFor(nameof(TargetMandatory))]
    [NotifyPropertyChangedFor(nameof(TargetOptional))]
    [NotifyPropertyChangedFor(nameof(TargetSavings))]

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasBudget;

    [ObservableProperty]
    private Budget _currentBudget;

    // Добавляем это свойство
    [ObservableProperty]
    private bool _hasNoBudget;

    // Расчетные свойства для UI (Идеальные суммы в деньгах)
    public decimal TargetMandatory => (CurrentBudget?.EstablishedAmount ?? 0) * (CurrentBudget?.NeedsPercentage ?? 0) / 100;
    public decimal TargetOptional => (CurrentBudget?.EstablishedAmount ?? 0) * (CurrentBudget?.WantsPercentage ?? 0) / 100;
    public decimal TargetSavings => (CurrentBudget?.EstablishedAmount ?? 0) * (CurrentBudget?.SavingsPercentage ?? 0) / 100;

    public BudgetViewModel(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [RelayCommand]
    public async Task LoadBudgetData()
    {
        // 1. Сначала обнуляем, чтобы UI увидел изменения при повторном заходе
        CurrentBudget = null;

        var budgetId = Session.CurrentUser?.Budgetid;

        if (budgetId != null && budgetId > 0)
        {
            // 2. Загружаем свежие данные из БД
            CurrentBudget = await _budgetService.GetBudgetByIdAsync(budgetId.Value);

            HasBudget = true;
            HasNoBudget = false;
        }
        else
        {
            HasBudget = false;
            HasNoBudget = true;
        }
    }


    [RelayCommand]
    private async Task CreateBudget()
    {
        // Shell сам достанет BudgetCreatePage из контейнера вместе со всеми сервисами
        await Shell.Current.GoToAsync(nameof(BudgetCreatePage));
    }
}