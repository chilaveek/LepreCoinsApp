using ApplicationCore.Interfaces;
using LepreCoins.Models;

namespace ApplicationCore.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Budget> BudgetRepository { get; }
        IRepository<Expense> ExpenseRepository { get; }
        IRepository<Income> IncomeRepository { get; }
        IRepository<Saving> SavingRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<Wallet> WalletRepository { get; }
        IRepository<TransferToSaving> TransferToSavingRepository { get; }

        Task<int> SaveChangesAsync();
    }
}