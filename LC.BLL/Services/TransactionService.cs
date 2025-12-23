namespace LepreCoins.Infrastructure.LC.BLL.Services;

using Interfaces.DTO;
using ApplicationCore.Interfaces;
using Interfaces;
using LepreCoins.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Create Methods

    public async Task<Result<TransactionDto>> AddExpenseAsync(CreateExpenseDto dto)
    {
        try
        {
            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(dto.WalletId);
            if (wallet == null) return Result<TransactionDto>.Failure("Кошелек не найден");

            if (wallet.Balance < dto.Sum)
                return Result<TransactionDto>.Failure("Недостаточно средств");

            var expense = new Expense
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Sum = dto.Sum,
                Description = dto.Description,
                Categoryid = dto.CategoryId,
                Userid = dto.UserId,
                Walletid = dto.WalletId
            };

            await _unitOfWork.ExpenseRepository.AddAsync(expense);

            wallet.Balance -= dto.Sum;
            await _unitOfWork.WalletRepository.UpdateAsync(wallet);

            await UpdateBudgetStatsAsync(dto.UserId, dto.CategoryId, dto.Sum);

            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при добавлении расхода: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> AddIncomeAsync(CreateIncomeDto dto)
    {
        try
        {
            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(dto.WalletId);
            if (wallet == null) return Result<TransactionDto>.Failure("Кошелек не найден");

            var income = new Income
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Sum = dto.Sum,
                Description = dto.Description,
                Incomecategoryid = dto.IncomeCategoryId,
                Userid = dto.UserId,
                Walletid = dto.WalletId
            };

            await _unitOfWork.IncomeRepository.AddAsync(income);

            wallet.Balance += dto.Sum;
            await _unitOfWork.WalletRepository.UpdateAsync(wallet);

            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionDto>.Success(MapToDto(income));
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при добавлении дохода: {ex.Message}");
        }
    }

    #endregion

    #region Update Methods

    public async Task<Result> UpdateTransactionAsync(int id, decimal amount, string description, int categoryId, int walletId)
    {
        // Сначала проверяем расходы
        var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
        if (expense != null)
        {
            // Используем позиционный конструктор record UpdateExpenseDto(decimal Sum, string Description, int CategoryId)
            return await UpdateExpenseAsync(id, new UpdateExpenseDto(amount, description, categoryId));
        }

        // Если не расход, проверяем доходы
        var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
        if (income != null)
        {
            // Используем позиционный конструктор record UpdateIncomeDto(decimal Sum, string Description, int IncomeCategoryId)
            return await UpdateIncomeAsync(id, new UpdateIncomeDto(amount, description, categoryId));
        }

        return Result.Failure("Транзакция для обновления не найдена");
    }

    public async Task<Result<TransactionDto>> UpdateExpenseAsync(int id, UpdateExpenseDto dto)
    {
        try
        {
            var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
            if (expense == null) return Result<TransactionDto>.Failure("Расход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(expense.Walletid ?? 0);
            if (wallet == null) return Result<TransactionDto>.Failure("Кошелек не найден");

            decimal oldSum = expense.Sum ?? 0;
            int oldCategoryId = expense.Categoryid ?? 0;
            decimal difference = dto.Sum - oldSum;

            if (wallet.Balance < difference)
                return Result<TransactionDto>.Failure("Недостаточно средств на кошельке");

            // Обновляем кошелек
            wallet.Balance -= difference;

            // Обновляем бюджет (откат старой суммы + применение новой)
            await UpdateBudgetStatsAsync(expense.Userid ?? 0, oldCategoryId, -oldSum);
            await UpdateBudgetStatsAsync(expense.Userid ?? 0, dto.CategoryId, dto.Sum);

            // Обновляем поля сущности
            expense.Sum = dto.Sum;
            expense.Description = dto.Description;
            expense.Categoryid = dto.CategoryId;

            await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            await _unitOfWork.ExpenseRepository.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionDto>.Success(MapToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при обновлении: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> UpdateIncomeAsync(int id, UpdateIncomeDto dto)
    {
        try
        {
            var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
            if (income == null) return Result<TransactionDto>.Failure("Доход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(income.Walletid ?? 0);
            if (wallet == null) return Result<TransactionDto>.Failure("Кошелек не найден");

            decimal oldSum = income.Sum ?? 0;
            decimal difference = dto.Sum - oldSum;

            income.Sum = dto.Sum;
            income.Description = dto.Description;
            income.Incomecategoryid = dto.IncomeCategoryId;

            wallet.Balance += difference;

            await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            await _unitOfWork.IncomeRepository.UpdateAsync(income);
            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionDto>.Success(MapToDto(income));
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при обновлении дохода: {ex.Message}");
        }
    }

    #endregion

    #region Delete Methods

    public async Task<Result> DeleteTransactionAsync(int id)
    {
        var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
        if (expense != null) return await DeleteExpenseAsync(id);

        var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
        if (income != null) return await DeleteIncomeAsync(id);

        return Result.Failure("Транзакция не найдена");
    }

    public async Task<Result> DeleteExpenseAsync(int id)
    {
        try
        {
            var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
            if (expense == null) return Result.Failure("Расход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(expense.Walletid ?? 0);
            if (wallet != null)
            {
                wallet.Balance += expense.Sum ?? 0;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            }

            // Откат бюджета
            await UpdateBudgetStatsAsync(expense.Userid ?? 0, expense.Categoryid ?? 0, -(expense.Sum ?? 0));

            await _unitOfWork.ExpenseRepository.DeleteAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при удалении: {ex.Message}");
        }
    }

    public async Task<Result> DeleteIncomeAsync(int id)
    {
        try
        {
            var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
            if (income == null) return Result.Failure("Доход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(income.Walletid ?? 0);
            if (wallet != null)
            {
                wallet.Balance -= income.Sum ?? 0;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            }

            await _unitOfWork.IncomeRepository.DeleteAsync(income);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при удалении дохода: {ex.Message}");
        }
    }

    #endregion

    public async Task<Result<IEnumerable<TransactionDto>>> GetUserTransactionsAsync(int userId, DateRange dateRange)
    {
        try
        {
            var expenses = await _unitOfWork.ExpenseRepository.FindAsync(e =>
                e.Userid == userId &&
                e.Date >= DateOnly.FromDateTime(dateRange.StartDate) &&
                e.Date <= DateOnly.FromDateTime(dateRange.EndDate));

            var incomes = await _unitOfWork.IncomeRepository.FindAsync(i =>
                i.Userid == userId &&
                i.Date >= DateOnly.FromDateTime(dateRange.StartDate) &&
                i.Date <= DateOnly.FromDateTime(dateRange.EndDate));

            var transactions = expenses.Select(MapToDto)
                .Concat(incomes.Select(MapToDto))
                .OrderByDescending(t => t.Date);

            return Result<IEnumerable<TransactionDto>>.Success(transactions);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TransactionDto>>.Failure($"Ошибка загрузки: {ex.Message}");
        }
    }

    #region Private Helpers

    private async Task UpdateBudgetStatsAsync(int userId, int categoryId, decimal amount)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user?.Budgetid == null) return;

        var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(user.Budgetid.Value);
        if (budget == null) return;

        budget.CurrentExpenses = (budget.CurrentExpenses ?? 0m) + amount;

        switch (categoryId)
        {
            case 1: budget.SpentNeeds = (budget.SpentNeeds ?? 0m) + amount; break;
            case 2: budget.SpentWants = (budget.SpentWants ?? 0m) + amount; break;
            case 3: budget.SpentSavings = (budget.SpentSavings ?? 0m) + amount; break;
        }

        await _unitOfWork.BudgetRepository.UpdateAsync(budget);
    }

    private TransactionDto MapToDto(Expense e) => new TransactionDto(
        e.Id,
        e.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
        e.Sum ?? 0,
        "Расход",
        e.Description ?? "",
        "Expense",
        e.Walletid ?? 0,
        "Кошелек");

    private TransactionDto MapToDto(Income i) => new TransactionDto(
        i.Id,
        i.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
        i.Sum ?? 0,
        "Доход",
        i.Description ?? "",
        "Income",
        i.Walletid ?? 0,
        "Кошелек");

    #endregion
}