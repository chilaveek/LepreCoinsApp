using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;
using LC.BLL.Session;
using LepreCoinsApp.Views;

namespace LepreCoinsApp.ViewModels;

public partial class BudgetCreateViewModel : ObservableObject
{
    private readonly IBudgetService _budgetService;

    public BudgetCreateViewModel(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsMoney), nameof(WantsMoney), nameof(SavingsMoney))]
    private decimal _establishedAmount = 50000;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsMoney))]
    private int _needsPercentage = 50;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WantsMoney))]
    private int _wantsPercentage = 30;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SavingsMoney))]
    private int _savingsPercentage = 20;

    public decimal NeedsMoney => EstablishedAmount * NeedsPercentage / 100;
    public decimal WantsMoney => EstablishedAmount * WantsPercentage / 100;
    public decimal SavingsMoney => EstablishedAmount * SavingsPercentage / 100;

    [RelayCommand]
    private async Task SaveBudget()
    {
        if (NeedsPercentage + WantsPercentage + SavingsPercentage != 100)
        {
            await App.Current.MainPage.DisplayAlertAsync("Внимание",
                $"Сумма должна быть 100%. Сейчас: {NeedsPercentage + WantsPercentage + SavingsPercentage}%", "ОК");
            return;
        }

        var dto = new CreateBudgetDto
        {
            Amount = EstablishedAmount,
            Needs = NeedsPercentage,
            Wants = WantsPercentage,
            Savings = SavingsPercentage,
            UserId = Session.CurrentUser.Id,
            PeriodStart = DateOnly.FromDateTime(DateTime.Now),
            PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddMonths(1))
        };

        var success = await _budgetService.CreateBudgetAsync(dto);
        if (success) await Shell.Current.GoToAsync("..");
    }
}