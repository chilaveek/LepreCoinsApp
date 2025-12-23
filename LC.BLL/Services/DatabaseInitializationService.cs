using LepreCoins.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LepreCoins.Infrastructure.LC.BLL.Services;

public interface IDatabaseInitializationService
{
    Task InitializeDatabaseAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly FamilybudgetdbContext _context;

    public DatabaseInitializationService(FamilybudgetdbContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
            await SeedCategoriesAsync();

            System.Diagnostics.Debug.WriteLine("Проверка базы данных завершена: всё готово к работе.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"КРИТИЧЕСКАЯ ОШИБКА БД: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Детали: {ex.InnerException.Message}");
            }
            throw;
        }
    }
    private async Task SeedCategoriesAsync()
    {
        if (!await _context.IncomeCategories.AnyAsync())
        {
            _context.IncomeCategories.AddRange(
                new IncomeCategory { Category = "Зарплата" },
                new IncomeCategory { Category = "Подарок" },
                new IncomeCategory { Category = "Перевод" },
                new IncomeCategory { Category = "Другое" }
            );
        }

        if (!await _context.ExpenseCategories.AnyAsync())
        {
            _context.ExpenseCategories.AddRange(
                new ExpenseCategory { Name = "Нужды" },      // Будет Id 1
                new ExpenseCategory { Name = "Хотелки" },    // Будет Id 2
                new ExpenseCategory { Name = "Копилка" }     // Будет Id 3
            );
        }

        await _context.SaveChangesAsync();
    }
}