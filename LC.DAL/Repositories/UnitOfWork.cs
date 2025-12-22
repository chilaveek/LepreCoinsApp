namespace LepreCoins.Infrastructure.LC.DAL;

using ApplicationCore.Interfaces;
using global::LC.DAL.Repositories;
using LepreCoins.Models;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly FamilybudgetdbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(FamilybudgetdbContext context)
    {
        _context = context;
    }

    public IRepository<Budget> BudgetRepository => GetRepository<Budget>();
    public IRepository<Expense> ExpenseRepository => GetRepository<Expense>();
    public IRepository<Income> IncomeRepository => GetRepository<Income>();
    public IRepository<Saving> SavingRepository => GetRepository<Saving>();
    public IRepository<User> UserRepository => GetRepository<User>();
    public IRepository<Wallet> WalletRepository => GetRepository<Wallet>();
    public IRepository<TransferToSaving> TransferToSavingRepository => GetRepository<TransferToSaving>();

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }

    private IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(EfRepository<>).MakeGenericType(type);
            var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
            _repositories.Add(type, repositoryInstance!);
        }
        return (IRepository<T>)_repositories[type];
    }
}
