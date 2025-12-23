using Interfaces;
using Interfaces.DTO;

namespace ApplicationCore.Interfaces
{
    public interface ITransactionService
    {
        // Создание
        Task<Result<TransactionDto>> AddExpenseAsync(CreateExpenseDto dto);
        Task<Result<TransactionDto>> AddIncomeAsync(CreateIncomeDto dto);

        // Обновление (конкретные типы)
        Task<Result<TransactionDto>> UpdateExpenseAsync(int id, UpdateExpenseDto dto);
        Task<Result<TransactionDto>> UpdateIncomeAsync(int id, UpdateIncomeDto dto);

        // Универсальный метод обновления (удобно для UI)
        Task<Result> UpdateTransactionAsync(int id, decimal amount, string description, int categoryId, int walletId);

        // Чтение
        Task<Result<IEnumerable<TransactionDto>>> GetUserTransactionsAsync(int userId, DateRange dateRange);

        // Удаление
        Task<Result> DeleteExpenseAsync(int id);
        Task<Result> DeleteIncomeAsync(int id);

        // Универсальный метод удаления
        Task<Result> DeleteTransactionAsync(int id);
    }
}