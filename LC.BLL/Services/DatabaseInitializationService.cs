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
}