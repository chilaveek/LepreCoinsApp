using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using LepreCoinsApp.ViewModels;
using System.Collections.ObjectModel;

namespace LepreCoinsApp.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly IReportService _reportService;
    private readonly IAuthenticationService _authService;
    private DateRange _currentDateRange = DateRange.CurrentMonth();

    [ObservableProperty]
    public ReportDataDto? reportData;

    [ObservableProperty]
    public decimal totalIncome;

    [ObservableProperty]
    public decimal totalExpenses;

    [ObservableProperty]
    public decimal netCash;

    [ObservableProperty]
    public bool isReportVisible = false;

    [ObservableProperty]
    public ObservableCollection<TransactionDto> reportTransactions = new();

    [ObservableProperty]
    public string selectedPeriod = "Текущий месяц";

    [ObservableProperty]
    public DateOnly periodStartDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    public DateOnly periodEndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

    [ObservableProperty]
    public bool isCustomPeriodVisible = false;

    [ObservableProperty]
    public string customDateError = "";

    public ReportsViewModel(
        IReportService reportService,
        IAuthenticationService authService)
    {
        _reportService = reportService;
        _authService = authService;
        Title = "Отчеты";
    }

    [RelayCommand]
    public Task SetCurrentMonthAsync()
    {
        _currentDateRange = DateRange.CurrentMonth();
        SelectedPeriod = "Текущий месяц";
        IsCustomPeriodVisible = false;
        UpdateDateLabels();
        return GenerateReportAsync();
    }

    [RelayCommand]
    public Task SetLast30DaysAsync()
    {
        _currentDateRange = DateRange.Last30Days();
        SelectedPeriod = "Последние 30 дней";
        IsCustomPeriodVisible = false;
        UpdateDateLabels();
        return GenerateReportAsync();
    }

    [RelayCommand]
    public Task SetCurrentYearAsync()
    {
        _currentDateRange = DateRange.CurrentYear();
        SelectedPeriod = "Текущий год";
        IsCustomPeriodVisible = false;
        UpdateDateLabels();
        return GenerateReportAsync();
    }

    [RelayCommand]
    public Task OpenCustomPeriodAsync()
    {
        IsCustomPeriodVisible = true;
        CustomDateError = "";
        PeriodStartDate = DateOnly.FromDateTime(_currentDateRange.StartDate);
        PeriodEndDate = DateOnly.FromDateTime(_currentDateRange.EndDate);
        SelectedPeriod = "Пользовательский период";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ApplyCustomPeriodAsync()
    {
        CustomDateError = "";

        // Валидация дат
        if (PeriodStartDate >= PeriodEndDate)
        {
            CustomDateError = "Дата начала должна быть раньше даты конца";
            return;
        }

        // Создаём новый DateRange
        _currentDateRange = new DateRange(
            PeriodStartDate.ToDateTime(TimeOnly.MinValue),
            PeriodEndDate.ToDateTime(TimeOnly.MaxValue)
        );

        IsCustomPeriodVisible = false;
        UpdateDateLabels();
        await GenerateReportAsync();
    }

    [RelayCommand]
    public Task CancelCustomPeriodAsync()
    {
        IsCustomPeriodVisible = false;
        CustomDateError = "";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task GenerateReportAsync()
    {
        try
        {
            IsBusy = true;
            BusyText = "Генерация отчета...";

            // Получаем ID пользователя из сервиса аутентификации
            int userId = _authService.GetCurrentUserId();

            // Вызываем BLL для получения данных отчёта
            var result = await _reportService.GetReportDataAsync(userId, _currentDateRange);

            if (result.IsSuccess && result.Data != null)
            {
                // Сохраняем данные в свойства
                ReportData = result.Data;
                TotalIncome = result.Data.TotalIncome;
                TotalExpenses = result.Data.TotalExpenses;
                NetCash = result.Data.NetCash;

                // Конвертируем транзакции в ObservableCollection для UI
                ReportTransactions = new ObservableCollection<TransactionDto>(
                    result.Data.Transactions.OrderByDescending(t => t.Date)
                );

                // Показываем отчёт на экране
                IsReportVisible = true;

                await Application.Current!.MainPage!.DisplayAlert(
                    "Успех",
                    $"Отчет сгенерирован за период {SelectedPeriod}",
                    "OK"
                );
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    result.ErrorMessage ?? "Не удалось сгенерировать отчет",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                $"Ошибка генерации отчета: {ex.Message}",
                "OK"
            );
        }
        finally
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    // ====== ЭКСПОРТ В PDF ======

    /// <summary>
    /// Сохраняет отчёт в PDF файл
    /// </summary>
    [RelayCommand]
    public async Task SaveReportToPdfAsync()
    {
        if (ReportData == null)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                "Сначала сгенерируйте отчет",
                "OK"
            );
            return;
        }

        try
        {
            IsBusy = true;
            BusyText = "Экспорт в PDF...";

            int userId = _authService.GetCurrentUserId();

            // Вызываем сервис для генерации PDF
            var result = await _reportService.GeneratePdfReportAsync(userId, _currentDateRange);

            if (result.IsSuccess && result.Data != null)
            {
                // Сохраняем PDF файл в папку AppDataDirectory
                var fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                // Записываем байты в файл
                await File.WriteAllBytesAsync(filePath, result.Data);

                // Пытаемся открыть файл (если возможно)
                await OpenPdfFileAsync(filePath);

                await Application.Current!.MainPage!.DisplayAlert(
                    "Успех",
                    $"Отчет сохранен:\n{fileName}\n\nПуть: {filePath}",
                    "OK"
                );
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    result.ErrorMessage ?? "Ошибка экспорта в PDF",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                $"Ошибка сохранения PDF: {ex.Message}",
                "OK"
            );
        }
        finally
        {
            IsBusy = false;
            BusyText = "";
        }
    }

    [RelayCommand]
    public Task CloseReportAsync()
    {
        IsReportVisible = false;
        ReportData = null;
        ReportTransactions = new ObservableCollection<TransactionDto>();
        TotalIncome = 0;
        TotalExpenses = 0;
        NetCash = 0;
        return Task.CompletedTask;
    }


    private void UpdateDateLabels()
    {
        PeriodStartDate = DateOnly.FromDateTime(_currentDateRange.StartDate);
        PeriodEndDate = DateOnly.FromDateTime(_currentDateRange.EndDate);
    }

    private async Task OpenPdfFileAsync(string filePath)
    {
        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch
        {
        }
    }
}
