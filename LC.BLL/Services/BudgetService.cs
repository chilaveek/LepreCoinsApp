using ApplicationCore.Interfaces;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using LepreCoins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LC.BLL.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public BudgetService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result<BudgetDto>> CreateBudgetAsync(CreateBudgetDto dto)
        {
            try
            {
                var budget = new Budget
                {
                    EstablishedAmount = dto.EstablishedAmount,
                    CurrentExpenses = 0,
                    PeriodStart = dto.PeriodStart,
                    PeriodEnd = dto.PeriodEnd
                };

                await _unitOfWork.BudgetRepository.AddAsync(budget);
                await _unitOfWork.SaveChangesAsync();

                var budgetDto = new BudgetDto(
                    budget.Id,
                    budget.EstablishedAmount ?? 0,
                    budget.CurrentExpenses ?? 0,
                    budget.PeriodStart ?? DateOnly.MinValue,
                    budget.PeriodEnd ?? DateOnly.MinValue,
                    new List<CategoryBudgetDto>());

                return Result<BudgetDto>.Success(budgetDto);
            }
            catch (Exception ex)
            {
                return Result<BudgetDto>.Failure($"Ошибка при создании бюджета: {ex.Message}");
            }
        }

        public async Task<Result<BudgetDto>> UpdateBudgetAsync(int budgetId, UpdateBudgetDto dto)
        {
            try
            {
                var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(budgetId);
                if (budget == null)
                    return Result<BudgetDto>.Failure("Бюджет не найден");

                budget.EstablishedAmount = dto.EstablishedAmount;
                budget.PeriodStart = dto.PeriodStart;
                budget.PeriodEnd = dto.PeriodEnd;

                await _unitOfWork.BudgetRepository.UpdateAsync(budget);
                await _unitOfWork.SaveChangesAsync();

                var budgetDto = new BudgetDto(
                    budget.Id,
                    budget.EstablishedAmount ?? 0,
                    budget.CurrentExpenses ?? 0,
                    budget.PeriodStart ?? DateOnly.MinValue,
                    budget.PeriodEnd ?? DateOnly.MinValue,
                    new List<CategoryBudgetDto>());

                return Result<BudgetDto>.Success(budgetDto);
            }
            catch (Exception ex)
            {
                return Result<BudgetDto>.Failure($"Ошибка при обновлении бюджета: {ex.Message}");
            }
        }

        public async Task<Result<BudgetDto>> GetBudgetAsync(int budgetId)
        {
            try
            {
                var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(budgetId);
                if (budget == null)
                    return Result<BudgetDto>.Failure("Бюджет не найден");

                var budgetDto = new BudgetDto(
                    budget.Id,
                    budget.EstablishedAmount ?? 0,
                    budget.CurrentExpenses ?? 0,
                    budget.PeriodStart ?? DateOnly.MinValue,
                    budget.PeriodEnd ?? DateOnly.MinValue,
                    new List<CategoryBudgetDto>());

                return Result<BudgetDto>.Success(budgetDto);
            }
            catch (Exception ex)
            {
                return Result<BudgetDto>.Failure($"Ошибка при получении бюджета: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает анализ бюджета с РЕАЛЬНЫМИ расходами из БД
        /// </summary>
        public async Task<Result<BudgetAnalysisDto>> GetBudgetAnalysisAsync(int budgetId)
        {
            try
            {
                var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(budgetId);
                if (budget == null)
                    return Result<BudgetAnalysisDto>.Failure("Бюджет не найден");

                // ВАЖНО: Берём РЕАЛЬНЫЕ расходы из базы данных
                // 1. Находим все расходы в период бюджета
                var expenses = await _unitOfWork.ExpenseRepository.FindAsync(e =>
                    e.Date >= budget.PeriodStart &&
                    e.Date <= budget.PeriodEnd);

                // 2. Обновляем CurrentExpenses в бюджете
                decimal totalSpent = expenses.Sum(e => e.Sum ?? 0);
                budget.CurrentExpenses = totalSpent;
                await _unitOfWork.BudgetRepository.UpdateAsync(budget);
                await _unitOfWork.SaveChangesAsync();

                // 3. Группируем расходы по категориям
                var expensesByCategory = expenses
                    .GroupBy(e => e.Categoryid)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Sum ?? 0));

                var categories = new List<CategoryAnalysisDto>();

                // 4. Создаём анализ по категориям
                foreach (var categoryId in expensesByCategory.Keys)
                {
                    var categorySpent = expensesByCategory[categoryId];
                    // В примере даём каждой категории по 10% от бюджета
                    var categoryLimit = (budget.EstablishedAmount ?? 0) * 0.1m;
                    double percentage = (double)(categoryLimit > 0 ? (categorySpent / categoryLimit) * 100 : 0);
                    var isExceeded = categorySpent > categoryLimit;

                    // Получаем название категории (если нужно)
                    string categoryName = $"Категория {categoryId}";

                    categories.Add(new CategoryAnalysisDto(
                        categoryName,
                        categoryLimit,
                        categorySpent,
                        percentage,
                        isExceeded));
                }

                // Если нет расходов, добавим пример категорий
                if (categories.Count == 0)
                {
                    categories.Add(new CategoryAnalysisDto("Продукты", 5000, 0, 0, false));
                    categories.Add(new CategoryAnalysisDto("Транспорт", 3000, 0, 0, false));
                    categories.Add(new CategoryAnalysisDto("Развлечения", 2000, 0, 0, false));
                }

                var totalBudget = budget.EstablishedAmount ?? 0;
                var remaining = totalBudget - totalSpent;
                var percentageUsed = totalBudget > 0 ? (double)(totalSpent / totalBudget) * 100 : 0;

                var analysis = new BudgetAnalysisDto(
                    totalBudget,
                    totalSpent,
                    remaining,
                    percentageUsed,
                    categories);

                return Result<BudgetAnalysisDto>.Success(analysis);
            }
            catch (Exception ex)
            {
                return Result<BudgetAnalysisDto>.Failure($"Ошибка при анализе бюджета: {ex.Message}");
            }
        }

        public async Task<Result> DeleteBudgetAsync(int budgetId)
        {
            try
            {
                var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(budgetId);
                if (budget == null)
                    return Result.Failure("Бюджет не найден");

                await _unitOfWork.BudgetRepository.DeleteAsync(budget);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Ошибка при удалении бюджета: {ex.Message}");
            }
        }

        public async Task<Result> CheckLowBalanceAlert(int walletId, decimal threshold)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(walletId);
                if (wallet == null)
                    return Result.Failure("Кошелек не найден");

                if (wallet.Balance.HasValue && wallet.Balance < threshold)
                {
                    await _notificationService.SendLowBalanceAlertAsync(
                        wallet.Userid ?? 0,
                        wallet.Name,
                        wallet.Balance.Value,
                        threshold);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Ошибка при проверке баланса: {ex.Message}");
            }
        }
    }
}