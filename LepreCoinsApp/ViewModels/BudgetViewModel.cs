using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;

namespace LepreCoinsApp.ViewModels
{
    public partial class BudgetViewModel : BaseViewModel
    {
        private readonly IBudgetService _budgetService;
        private int _currentBudgetId = 1;

        [ObservableProperty]
        public decimal budgetLimit = 50000;

        [ObservableProperty]
        public decimal spentAmount = 0;

        [ObservableProperty]
        public double percentageUsed = 0;

        [ObservableProperty]
        public DateOnly periodStart = DateOnly.FromDateTime(DateTime.Now);

        [ObservableProperty]
        public DateOnly periodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

        [ObservableProperty]
        public bool isEditPopupVisible = false;

        [ObservableProperty]
        public string budgetLimitText = "50000";

        [ObservableProperty]
        public string editError = "";

        [ObservableProperty]
        public BudgetAnalysisDto? budgetAnalysis;

        public BudgetViewModel(IBudgetService budgetService)
        {
            _budgetService = budgetService;
            Title = "Бюджет";
        }


        [RelayCommand]
        public async Task InitializeBudgetAsync()
        {
            try
            {
                IsBusy = true;
                BusyText = "Инициализация бюджета...";

                var getResult = await _budgetService.GetBudgetAsync(_currentBudgetId);

                if (getResult.IsSuccess && getResult.Data != null)
                {

                    BudgetLimit = getResult.Data.EstablishedAmount;
                    PeriodStart = getResult.Data.PeriodStart;
                    PeriodEnd = getResult.Data.PeriodEnd;
                    BudgetLimitText = BudgetLimit.ToString("F0");
                }
                else
                {
       
                    var createDto = new CreateBudgetDto(
                        BudgetLimit,
                        PeriodStart,
                        PeriodEnd,
                        new List<int> { 1 });

                    var createResult = await _budgetService.CreateBudgetAsync(createDto);

                    if (createResult.IsSuccess && createResult.Data != null)
                    {
                        _currentBudgetId = createResult.Data.Id;
                        BudgetLimit = createResult.Data.EstablishedAmount;
                        PeriodStart = createResult.Data.PeriodStart;
                        PeriodEnd = createResult.Data.PeriodEnd;
                        BudgetLimitText = BudgetLimit.ToString("F0");
                    }
                    else
                    {
                        await Application.Current!.MainPage!.DisplayAlertAsync("Ошибка",
                            "Не удалось создать бюджет", "OK");
                    }
                }

                // 2. Загружаем анализ (расходы по категориям)
                await LoadBudgetAnalysisAsync();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync("Ошибка",
                    $"Ошибка инициализации: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadBudgetAnalysisAsync()
        {
            try
            {
                IsBusy = true;
                BusyText = "Загрузка статистики...";

                var result = await _budgetService.GetBudgetAnalysisAsync(_currentBudgetId);

                if (result.IsSuccess && result.Data != null)
                {
                    BudgetAnalysis = result.Data;
                    BudgetLimit = result.Data.TotalBudget;
                    SpentAmount = result.Data.TotalSpent;
                    PercentageUsed = result.Data.PercentageUsed;
                    BudgetLimitText = result.Data.TotalBudget.ToString("F0");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Ошибка",
                        result.ErrorMessage ?? "Не удалось загрузить статистику", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка",
                    $"Ошибка загрузки: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void OpenEditPopup()
        {
            EditError = "";
            BudgetLimitText = BudgetLimit.ToString("F0");
            IsEditPopupVisible = true;
        }

        [RelayCommand]
        public void CloseEditPopup()
        {
            IsEditPopupVisible = false;
            EditError = "";
        }

        [RelayCommand]
        public async Task SaveBudgetAsync()
        {
            try
            {
                EditError = "";

                // Валидация лимита
                if (string.IsNullOrWhiteSpace(BudgetLimitText))
                {
                    EditError = "Введите лимит бюджета";
                    return;
                }

                if (!decimal.TryParse(BudgetLimitText, out var newLimit))
                {
                    EditError = "Лимит должен быть числом";
                    return;
                }

                if (newLimit <= 0)
                {
                    EditError = "Лимит должен быть больше 0";
                    return;
                }

                // Валидация дат
                if (PeriodStart >= PeriodEnd)
                {
                    EditError = "Дата начала должна быть раньше даты конца";
                    return;
                }

                // Сохраняем в БД
                IsBusy = true;
                BusyText = "Сохранение...";

                var updateDto = new UpdateBudgetDto(newLimit, PeriodStart, PeriodEnd);
                var result = await _budgetService.UpdateBudgetAsync(_currentBudgetId, updateDto);

                if (result.IsSuccess)
                {
                    BudgetLimit = newLimit;

                    // Пересчитываем процент
                    if (BudgetLimit > 0)
                    {
                        PercentageUsed = (double)(SpentAmount / BudgetLimit) * 100;
                    }

                    IsEditPopupVisible = false;

                    // Перезагружаем анализ
                    await LoadBudgetAnalysisAsync();

                    await Application.Current!.MainPage!.DisplayAlert("Успех",
                        "Бюджет успешно обновлён!", "OK");
                }
                else
                {
                    EditError = result.ErrorMessage ?? "Ошибка сохранения";
                }
            }
            catch (Exception ex)
            {
                EditError = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshStatisticsAsync()
        {
            await LoadBudgetAnalysisAsync();
        }
    }
}