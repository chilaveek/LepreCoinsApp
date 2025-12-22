using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;
using LepreCoins.Infrastructure.LC.DAL;
using LepreCoins.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace LepreCoinsApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly ITransactionService _transactionService;
    private readonly IWalletService _walletService;
    private readonly IReceiptScannerService _receiptScanner;

    private int _currentUserId = 1;

    [ObservableProperty]
    private ObservableCollection<TransactionDto> transactions = new();

    [ObservableProperty]
    private decimal totalBalance;

    [ObservableProperty]
    private decimal monthlyIncome;

    [ObservableProperty]
    private decimal monthlyExpenses;

    public record LookupItem(int Id, string Title);

    [ObservableProperty]
    private bool isTransactionPopupVisible;

    [ObservableProperty]
    private string transactionPopupTitle = string.Empty;

    [ObservableProperty]
    private bool isIncomeMode;

    [ObservableProperty]
    private string transactionAmountText = string.Empty;

    [ObservableProperty]
    private string transactionDescription = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WalletDto> wallets = new();

    [ObservableProperty]
    private WalletDto? selectedWallet;

    [ObservableProperty]
    private ObservableCollection<LookupItem> categories = new();

    [ObservableProperty]
    private LookupItem? selectedCategory;

    [ObservableProperty]
    private string transactionFormError = string.Empty;

    public bool HasTransactionFormError => !string.IsNullOrWhiteSpace(TransactionFormError);

    partial void OnTransactionFormErrorChanged(string value) =>
        OnPropertyChanged(nameof(HasTransactionFormError));

    public MainViewModel(
        ITransactionService transactionService,
        IWalletService walletService,
        IReceiptScannerService receiptScanner)
    {
        _transactionService = transactionService;
        _walletService = walletService;
        _receiptScanner = receiptScanner;

        Title = "Главная";
    }


    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        BusyText = "Загрузка...";
        try
        {
            // 1) Кошельки пользователя
            var walletsResult = await _walletService.GetUserWalletsAsync(_currentUserId);
            if (walletsResult.IsSuccess && walletsResult.Data != null)
            {
                Wallets = new ObservableCollection<WalletDto>(walletsResult.Data);
                SelectedWallet ??= Wallets.FirstOrDefault();
            }
            else
            {
                Wallets = new ObservableCollection<WalletDto>();
                SelectedWallet = null;
            }

            // 2) Транзакции пользователя за месяц
            var transactionsResult = await _transactionService.GetUserTransactionsAsync(
                _currentUserId,
                DateRange.CurrentMonth());
            if (transactionsResult.IsSuccess && transactionsResult.Data != null)
                Transactions = new ObservableCollection<TransactionDto>(transactionsResult.Data);
            else
                Transactions = new ObservableCollection<TransactionDto>();

            // 3) Итоги месяца
            MonthlyIncome = Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            MonthlyExpenses = Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            TotalBalance = Wallets.Sum(w => w.Balance);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    private void OpenTransactionPopup(bool incomeMode)
    {
        IsIncomeMode = incomeMode;
        TransactionPopupTitle = incomeMode ? "Добавить доход" : "Добавить расход";

        TransactionAmountText = "";
        TransactionDescription = "";

        Categories = incomeMode
            ? new ObservableCollection<LookupItem>(new[]
              {
                  new LookupItem(1, "Зарплата"),
                  new LookupItem(2, "Фриланс"),
                  new LookupItem(3, "Подарок"),
                  new LookupItem(4, "Другое")
              })
            : new ObservableCollection<LookupItem>(new[]
              {
                  new LookupItem(1, "Продукты"),
                  new LookupItem(2, "Транспорт"),
                  new LookupItem(3, "Дом"),
                  new LookupItem(4, "Здоровье"),
                  new LookupItem(5, "Развлечения")
              });

        SelectedCategory ??= Categories.FirstOrDefault();
        SelectedWallet ??= Wallets.FirstOrDefault();

        TransactionFormError = "";
        IsTransactionPopupVisible = true;
    }

    [RelayCommand]
    private void CancelTransaction()
    {
        IsTransactionPopupVisible = false;
        TransactionFormError = "";
    }

    [RelayCommand]
    public Task AddIncomeAsync()
    {
        OpenTransactionPopup(incomeMode: true);
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task AddExpenseAsync()
    {
        OpenTransactionPopup(incomeMode: false);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        TransactionFormError = "";
        if (SelectedWallet == null)
        {
            TransactionFormError = "Выберите кошелёк.";
            return;
        }

        if (SelectedCategory == null)
        {
            TransactionFormError = "Выберите категорию.";
            return;
        }

        var normalized = (TransactionAmountText ?? "").Trim().Replace(",", ".");
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            TransactionFormError = "Введите корректную сумму (> 0).";
            return;
        }

        var desc = (TransactionDescription ?? "").Trim();
        if (string.IsNullOrWhiteSpace(desc))
            desc = IsIncomeMode ? "Доход" : "Расход";

        IsBusy = true;
        BusyText = "Сохранение...";
        try
        {
            if (IsIncomeMode)
            {
                var dto = new CreateIncomeDto(
                    amount,
                    desc,
                    SelectedCategory.Id,
                    _currentUserId,
                    SelectedWallet.Id
                );
                var res = await _transactionService.AddIncomeAsync(dto);
                if (!res.IsSuccess)
                {
                    TransactionFormError = res.ErrorMessage;
                    return;
                }
            }
            else
            {
                var dto = new CreateExpenseDto(
                    UserId: _currentUserId,
                    WalletId: SelectedWallet.Id,
                    CategoryId: SelectedCategory.Id,
                    Sum: amount,
                    Description: desc
                );
                var res = await _transactionService.AddExpenseAsync(dto);
                if (!res.IsSuccess)
                {
                    TransactionFormError = res.ErrorMessage;
                    return;
                }
            }

            IsTransactionPopupVisible = false;
            await LoadDataAsync(); 
        }
        catch (Exception ex)
        {
            TransactionFormError = ex.Message;
        }
        finally
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    [RelayCommand]
    public async Task DeleteWalletAsync(WalletDto wallet)
    {
        if (wallet == null)
            return;

        var confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Подтверждение",
            $"Вы действительно хотите удалить кошелек \"{wallet.Name}\"?",
            "Да",
            "Нет");

        if (!confirmed)
            return;

        IsBusy = true;
        BusyText = "Удаление кошелька...";

        try
        {
            var result = await _walletService.DeleteWalletAsync(wallet.Id);
            if (!result.IsSuccess)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    result.ErrorMessage,
                    "OK");
                return;
            }
            _walletService.DeleteWalletAsync(wallet.Id);
            Wallets.Remove(wallet);

            await Application.Current!.MainPage!.DisplayAlert(
                "Успех",
                $"Кошелек \"{wallet.Name}\" удален",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                $"Ошибка при удалении: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    [RelayCommand]
    public async Task ScanReceiptAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Камера не поддерживается.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return;

            using var stream = await photo.OpenReadAsync();
            var result = await _receiptScanner.ScanReceiptAsync(stream);

            if (!result.IsSuccess || result.Data == null)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", result.ErrorMessage, "OK");
                return;
            }

            var receipt = result.Data;

            // Предзаполним форму расхода
            OpenTransactionPopup(incomeMode: false);

            TransactionAmountText = (receipt.TotalAmount ?? 0m)
                .ToString("0.##", CultureInfo.InvariantCulture);

            TransactionDescription = "Чек";
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}
