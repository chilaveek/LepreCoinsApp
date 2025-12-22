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
            // Логируем критическую ошибку
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
        // Доходы
        if (!await _context.IncomeCategories.AnyAsync())
        {
            _context.IncomeCategories.AddRange(
                new IncomeCategory { Category = "Зарплата" },   // Id будет 1
                new IncomeCategory { Category = "Фриланс" },   // 2
                new IncomeCategory { Category = "Подарок" },   // 3
                new IncomeCategory { Category = "Другое" }     // 4
            );
        }

        // Расходы
        if (!await _context.ExpenseCategories.AnyAsync())
        {
            _context.ExpenseCategories.AddRange(
                new ExpenseCategory { Name = "Продукты" },      // Id 1
                new ExpenseCategory { Name = "Транспорт" },     // 2
                new ExpenseCategory { Name = "Дом" },           // 3
                new ExpenseCategory { Name = "Здоровье" },      // 4
                new ExpenseCategory { Name = "Развлечения" }    // 5
            );
        }

        await _context.SaveChangesAsync();
    }
}