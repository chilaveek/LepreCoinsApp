using LC.BLL.Session;
using ApplicationCore.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;
using System.Collections.ObjectModel;
using System.Globalization;
using Interfaces;

namespace LepreCoinsApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly ITransactionService _transactionService;
    private readonly IWalletService _walletService;
    private readonly IReceiptScannerService _receiptScanner;

    private int _currentUserId = Session.CurrentUser.Id;

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

    // Свойство для хранения редактируемой транзакции
    [ObservableProperty]
    private TransactionDto? editingTransaction;

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
            var walletsResult = await _walletService.GetUserWalletsAsync(_currentUserId);
            if (walletsResult.IsSuccess && walletsResult.Data != null)
            {
                Wallets = new ObservableCollection<WalletDto>(walletsResult.Data);
                SelectedWallet ??= Wallets.FirstOrDefault();
            }

            var transactionsResult = await _transactionService.GetUserTransactionsAsync(
                _currentUserId,
                DateRange.CurrentMonth());

            if (transactionsResult.IsSuccess && transactionsResult.Data != null)
                Transactions = new ObservableCollection<TransactionDto>(transactionsResult.Data);

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
        }
    }

    private void OpenTransactionPopup(bool incomeMode)
    {
        IsIncomeMode = incomeMode;
        if (EditingTransaction == null)
        {
            TransactionPopupTitle = incomeMode ? "Добавить доход" : "Добавить расход";
            TransactionAmountText = "";
            TransactionDescription = "";
        }

        Categories = incomeMode
            ? new ObservableCollection<LookupItem>(new[]
              {
                  new LookupItem(1, "Зарплата"),
                  new LookupItem(2, "Подарок"),
                  new LookupItem(3, "Перевод"),
                  new LookupItem(4, "Другое")
              })
            : new ObservableCollection<LookupItem>(new[]
              {
                  new LookupItem(1, "Нужды"),
                  new LookupItem(2, "Хотелки"),
                  new LookupItem(3, "Копилка")
              });

        SelectedCategory = Categories.FirstOrDefault();
        IsTransactionPopupVisible = true;
    }

    [RelayCommand]
    public void EditTransaction(TransactionDto transaction)
    {
        if (transaction == null) return;

        EditingTransaction = transaction;
        TransactionPopupTitle = "Редактировать";
        TransactionAmountText = transaction.Amount.ToString(CultureInfo.InvariantCulture);
        TransactionDescription = transaction.Description;

        SelectedWallet = Wallets.FirstOrDefault(w => w.Id == transaction.WalletId);

        OpenTransactionPopup(transaction.Type == "Income");
    }

    [RelayCommand]
    public async Task DeleteTransactionAsync(TransactionDto transaction)
    {
        if (transaction == null) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Подтверждение", "Удалить эту операцию?", "Да", "Нет");

        if (!confirm) return;

        IsBusy = true;
        var result = await _transactionService.DeleteTransactionAsync(transaction.Id);
        if (result.IsSuccess)
        {
            await LoadDataAsync();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", result.ErrorMessage, "OK");
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (SelectedWallet == null || SelectedCategory == null)
        {
            TransactionFormError = "Заполните все поля.";
            return;
        }

        var normalized = (TransactionAmountText ?? "").Trim().Replace(",", ".");
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            TransactionFormError = "Введите корректную сумму.";
            return;
        }

        IsBusy = true;
        try
        {
            Result res;
            if (EditingTransaction != null)
            {
                // ЛОГИКА ОБНОВЛЕНИЯ
                res = await _transactionService.UpdateTransactionAsync(
                    EditingTransaction.Id,
                    amount,
                    TransactionDescription,
                    SelectedCategory.Id,
                    SelectedWallet.Id);
            }
            else if (IsIncomeMode)
            {
                res = await _transactionService.AddIncomeAsync(new CreateIncomeDto(
                    amount, TransactionDescription, SelectedCategory.Id, _currentUserId, SelectedWallet.Id));
            }
            else
            {
                res = await _transactionService.AddExpenseAsync(new CreateExpenseDto(
                    amount, TransactionDescription, _currentUserId, SelectedWallet.Id, SelectedCategory.Id));
            }

            if (res.IsSuccess)
            {
                CancelTransaction();
                await LoadDataAsync();
            }
            else
            {
                TransactionFormError = res.ErrorMessage;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelTransaction()
    {
        IsTransactionPopupVisible = false;
        EditingTransaction = null;
        TransactionFormError = "";
    }

    [RelayCommand]
    public Task AddIncomeAsync() { EditingTransaction = null; OpenTransactionPopup(true); return Task.CompletedTask; }

    [RelayCommand]
    public Task AddExpenseAsync() { EditingTransaction = null; OpenTransactionPopup(false); return Task.CompletedTask; }

    [RelayCommand]
    public async Task ScanReceiptAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return;
            using var stream = await photo.OpenReadAsync();
            var result = await _receiptScanner.ScanReceiptAsync(stream);
            if (result.IsSuccess && result.Data != null)
            {
                EditingTransaction = null;
                OpenTransactionPopup(false);
                TransactionAmountText = (result.Data.TotalAmount ?? 0m).ToString(CultureInfo.InvariantCulture);
                TransactionDescription = "Чек";
            }
        }
        catch (Exception ex) { await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "OK"); }
    }
}