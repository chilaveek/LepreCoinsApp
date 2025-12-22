using Interfaces;
using Interfaces.DTO;
namespace ApplicationCore.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<TransactionDto>> AddExpenseAsync(CreateExpenseDto dto);
        Task<Result<TransactionDto>> AddIncomeAsync(CreateIncomeDto dto);
        Task<Result<TransactionDto>> UpdateExpenseAsync(int id, UpdateExpenseDto dto);
        Task<Result<TransactionDto>> UpdateIncomeAsync(int id, UpdateIncomeDto dto);
        Task<Result<IEnumerable<TransactionDto>>> GetUserTransactionsAsync(int userId, DateRange dateRange);
        Task<Result> DeleteExpenseAsync(int id);
        Task<Result> DeleteIncomeAsync(int id);
    }
}