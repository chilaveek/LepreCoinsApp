namespace LepreCoins.Infrastructure.LC.BLL.Services;

using Interfaces.DTO;
using ApplicationCore.Interfaces;
using Interfaces;
using LepreCoins.Models;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransactionDto>> AddExpenseAsync(CreateExpenseDto dto)
    {
        try
        {
            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(dto.WalletId);
            if (wallet == null)
                return Result<TransactionDto>.Failure("Кошелек не найден");

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

            await _unitOfWork.SaveChangesAsync();

            var transactionDto = new TransactionDto(
                expense.Id,
                expense.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                expense.Sum ?? 0,
                "Расход",
                expense.Description ?? "",
                "Expense");

            return Result<TransactionDto>.Success(transactionDto);
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
            if (wallet == null)
                return Result<TransactionDto>.Failure("Кошелек не найден");

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

            var transactionDto = new TransactionDto(
                income.Id,
                income.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                income.Sum ?? 0,
                "Доход",
                income.Description ?? "",
                "Income");

            return Result<TransactionDto>.Success(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при добавлении дохода: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> UpdateExpenseAsync(int id, UpdateExpenseDto dto)
    {
        try
        {
            var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
            if (expense == null)
                return Result<TransactionDto>.Failure("Расход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(expense.Walletid ?? 0);
            if (wallet == null)
                return Result<TransactionDto>.Failure("Кошелек не найден");

            var oldSum = expense.Sum ?? 0;
            decimal difference = dto.Sum - oldSum;

            if (wallet.Balance < difference)
                return Result<TransactionDto>.Failure("Недостаточно средств");

            expense.Sum = dto.Sum;
            expense.Description = dto.Description;
            expense.Categoryid = dto.CategoryId;

            wallet.Balance -= difference;
            await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            await _unitOfWork.ExpenseRepository.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            var transactionDto = new TransactionDto(
                expense.Id,
                expense.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                expense.Sum ?? 0,
                "Расход",
                expense.Description ?? "",
                "Expense");

            return Result<TransactionDto>.Success(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при обновлении расхода: {ex.Message}");
        }
    }

    public async Task<Result<TransactionDto>> UpdateIncomeAsync(int id, UpdateIncomeDto dto)
    {
        try
        {
            var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
            if (income == null)
                return Result<TransactionDto>.Failure("Доход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(income.Walletid ?? 0);
            if (wallet == null)
                return Result<TransactionDto>.Failure("Кошелек не найден");

            var oldSum = income.Sum ?? 0;
            decimal difference = dto.Sum - oldSum;

            income.Sum = dto.Sum;
            income.Description = dto.Description;
            income.Incomecategoryid = dto.IncomeCategoryId;

            wallet.Balance += difference;
            await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            await _unitOfWork.IncomeRepository.UpdateAsync(income);
            await _unitOfWork.SaveChangesAsync();

            var transactionDto = new TransactionDto(
                income.Id,
                income.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                income.Sum ?? 0,
                "Доход",
                income.Description ?? "",
                "Income");

            return Result<TransactionDto>.Success(transactionDto);
        }
        catch (Exception ex)
        {
            return Result<TransactionDto>.Failure($"Ошибка при обновлении дохода: {ex.Message}");
        }
    }

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

            var transactions = new List<TransactionDto>();

            foreach (var expense in expenses)
            {
                transactions.Add(new TransactionDto(
                    expense.Id,
                    expense.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                    expense.Sum ?? 0,
                    "Расход",
                    expense.Description ?? "",
                    "Expense"));
            }

            foreach (var income in incomes)
            {
                transactions.Add(new TransactionDto(
                    income.Id,
                    income.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                    income.Sum ?? 0,
                    "Доход",
                    income.Description ?? "",
                    "Income"));
            }

            return Result<IEnumerable<TransactionDto>>.Success(transactions.OrderByDescending(t => t.Date));
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TransactionDto>>.Failure($"Ошибка при получении транзакций: {ex.Message}");
        }
    }

    public async Task<Result> DeleteExpenseAsync(int id)
    {
        try
        {
            var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
            if (expense == null)
                return Result.Failure("Расход не найден");

            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(expense.Walletid ?? 0);
            if (wallet != null)
            {
                wallet.Balance += expense.Sum ?? 0;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);
            }

            await _unitOfWork.ExpenseRepository.DeleteAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при удалении расхода: {ex.Message}");
        }
    }

    public async Task<Result> DeleteIncomeAsync(int id)
    {
        try
        {
            var income = await _unitOfWork.IncomeRepository.GetByIdAsync(id);
            if (income == null)
                return Result.Failure("Доход не найден");

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
}