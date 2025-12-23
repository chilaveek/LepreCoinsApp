using LC.BLL.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using System.Collections.ObjectModel;

namespace LepreCoinsApp.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly IReportService _reportService;
    private DateRange _currentDateRange;
    private readonly List<TransactionDto> _allCurrentTransactions = new();

    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpenses;
    [ObservableProperty] private decimal netCash;
    [ObservableProperty] private bool isReportVisible = false;
    [ObservableProperty] private DateTime periodStartDate = DateTime.Now.AddMonths(-1);
    [ObservableProperty] private DateTime periodEndDate = DateTime.Now;
    [ObservableProperty] private string customDateError = "";
    [ObservableProperty] private string maxExpenseCategory = "—";
    [ObservableProperty] private decimal maxExpenseAmount;
    [ObservableProperty] private string maxIncomeCategory = "—";
    [ObservableProperty] private decimal maxIncomeAmount;
    [ObservableProperty] private string selectedSortType = "Сначала новые";
    public ObservableCollection<TransactionDto> ReportTransactions { get; } = new();

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [RelayCommand]
    public async Task GenerateReportAsync()
    {
        CustomDateError = "";
        if (PeriodStartDate >= PeriodEndDate)
        {
            CustomDateError = "Дата начала должна быть меньше даты конца";
            IsReportVisible = false;
            return;
        }

        IsBusy = true;
        try
        {
            _currentDateRange = new DateRange(PeriodStartDate, PeriodEndDate);
            var result = await _reportService.GetReportDataAsync(Session.CurrentUser.Id, _currentDateRange);

            if (result.IsSuccess && result.Data != null)
            {
                TotalIncome = result.Data.TotalIncome;
                TotalExpenses = result.Data.TotalExpenses;
                NetCash = result.Data.NetCash;

                _allCurrentTransactions.Clear();
                _allCurrentTransactions.AddRange(result.Data.Transactions);

                UpdateHighlights();

                ApplySort();

                IsReportVisible = true;
            }
            else
            {
                IsReportVisible = false;
                await Application.Current.MainPage.DisplayAlert("Инфо", "Нет данных", "OK");
            }
        }
        finally { IsBusy = false; }
    }

    private void UpdateHighlights()
    {
        var maxExp = _allCurrentTransactions.Where(t => t.Type == "Расход" || t.Type == "Expense").OrderByDescending(t => t.Amount).FirstOrDefault();
        MaxExpenseCategory = maxExp?.Description ?? "—";
        MaxExpenseAmount = maxExp?.Amount ?? 0;

        var maxInc = _allCurrentTransactions.Where(t => t.Type == "Доход" || t.Type == "Income").OrderByDescending(t => t.Amount).FirstOrDefault();
        MaxIncomeCategory = maxInc?.Description ?? "—";
        MaxIncomeAmount = maxInc?.Amount ?? 0;
    }

    partial void OnSelectedSortTypeChanged(string value) => ApplySort();

    private void ApplySort()
    {
        if (!_allCurrentTransactions.Any()) return;

        var sorted = SelectedSortType switch
        {
            "Сначала дорогие" => _allCurrentTransactions.OrderByDescending(t => t.Amount),
            "Сначала дешевые" => _allCurrentTransactions.OrderBy(t => t.Amount),
            "Сначала доходы" => _allCurrentTransactions.OrderByDescending(t => t.Type == "Доход" || t.Type == "Income").ThenByDescending(t => t.Date),
            "Сначала расходы" => _allCurrentTransactions.OrderByDescending(t => t.Type == "Расход" || t.Type == "Expense").ThenByDescending(t => t.Date),
            _ => _allCurrentTransactions.OrderByDescending(t => t.Date)
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ReportTransactions.Clear();
            foreach (var t in sorted)
                ReportTransactions.Add(t);
        });
    }

    [RelayCommand]
    public async Task SaveReportToPdfAsync()
    {
        if (!IsReportVisible) return;
        IsBusy = true;
        try
        {
            var result = await _reportService.GeneratePdfReportAsync(Session.CurrentUser.Id, _currentDateRange);
            if (result.IsSuccess)
            {
                var filePath = Path.Combine(FileSystem.CacheDirectory, "Report.pdf");
                await File.WriteAllBytesAsync(filePath, result.Data);
                await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(filePath) });
            }
        }
        finally { IsBusy = false; }
    }
}