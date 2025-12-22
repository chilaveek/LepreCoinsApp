using LepreCoins.Models;
using LC.DAL;
using Microsoft.EntityFrameworkCore;

namespace LC.BLL.Services;

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
            // Создаём БД если её нет
            await _context.Database.EnsureCreatedAsync();

            // Проверяем, есть ли уже данные
            if (await _context.Users.AnyAsync())
            {
                return; // БД уже инициализирована
            }

            // Создаём тестового пользователя
            var testUser = new User
            {
                Name = "Иван Петров",
                Email = "ivan@example.com",
                PasswordHash = "hashed_password_123", // В реальном приложении хеш пароля
                Familyid = null
            };

            await _context.Users.AddAsync(testUser);
            await _context.SaveChangesAsync();

            // Создаём кошельки для пользователя
            var wallets = new List<Wallet>
            {
                new Wallet
                {
                    Name = "Основной счёт",
                    Balance = 50000m,
                    Currency = "RUB",
                    Userid = testUser.Id
                },
                new Wallet
                {
                    Name = "Карта Сбербанка",
                    Balance = 25000m,
                    Currency = "RUB",
                    Userid = testUser.Id
                },
                new Wallet
                {
                    Name = "Резервный фонд",
                    Balance = 100000m,
                    Currency = "RUB",
                    Userid = testUser.Id
                },
                new Wallet
                {
                    Name = "Путешествия",
                    Balance = 15000m,
                    Currency = "RUB",
                    Userid = testUser.Id
                }
            };

            await _context.Wallets.AddRangeAsync(wallets);
            await _context.SaveChangesAsync();

            // Создаём категории расходов
            var expenseCategories = new List<ExpenseCategory>
            {
                new ExpenseCategory { Name = "Продукты", CategoryType = "Food", Attribute = "🛒" },
                new ExpenseCategory { Name = "Транспорт", CategoryType = "Transport", Attribute = "🚗" },
                new ExpenseCategory { Name = "Развлечения", CategoryType = "Entertainment", Attribute = "🎬" },
                new ExpenseCategory { Name = "Коммунальные услуги", CategoryType = "Utilities", Attribute = "💡" },
                new ExpenseCategory { Name = "Здоровье", CategoryType = "Health", Attribute = "⚕️" }
            };

            await _context.ExpenseCategories.AddRangeAsync(expenseCategories);
            await _context.SaveChangesAsync();

            // Создаём категории доходов
            var incomeCategories = new List<IncomeCategory>
            {
                new IncomeCategory { Category = "Зарплата" },
                new IncomeCategory { Category = "Фриланс" },
                new IncomeCategory { Category = "Инвестиции" },
                new IncomeCategory { Category = "Прочее" }
            };

            await _context.IncomeCategories.AddRangeAsync(incomeCategories);
            await _context.SaveChangesAsync();

            // Добавляем тестовые транзакции (доходы)
            var incomes = new List<Income>
            {
                new Income
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                    Sum = 75000m,
                    Description = "Зарплата за декабрь",
                    Userid = testUser.Id,
                    Incomecategoryid = incomeCategories[0].Id,
                    Walletid = wallets[0].Id
                },
                new Income
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                    Sum = 12000m,
                    Description = "Фриланс проект",
                    Userid = testUser.Id,
                    Incomecategoryid = incomeCategories[1].Id,
                    Walletid = wallets[1].Id
                }
            };

            await _context.Incomes.AddRangeAsync(incomes);
            await _context.SaveChangesAsync();

            // Добавляем тестовые расходы
            var expenses = new List<Expense>
            {
                new Expense
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-7)),
                    Sum = 5000m,
                    Description = "Покупка продуктов в супермаркете",
                    Userid = testUser.Id,
                    Categoryid = expenseCategories[0].Id,
                    Walletid = wallets[0].Id
                },
                new Expense
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-4)),
                    Sum = 2000m,
                    Description = "Бензин",
                    Userid = testUser.Id,
                    Categoryid = expenseCategories[1].Id,
                    Walletid = wallets[0].Id
                },
                new Expense
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Sum = 1500m,
                    Description = "Кинотеатр с друзьями",
                    Userid = testUser.Id,
                    Categoryid = expenseCategories[2].Id,
                    Walletid = wallets[1].Id
                },
                new Expense
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Sum = 3500m,
                    Description = "Электричество и вода",
                    Userid = testUser.Id,
                    Categoryid = expenseCategories[3].Id,
                    Walletid = wallets[0].Id
                }
            };

            await _context.Expenses.AddRangeAsync(expenses);
            await _context.SaveChangesAsync();

            // Создаём цель сбережений
            var saving = new Saving
            {
                GoalName = "Отпуск в Турцию",
                TargetAmount = 200000m,
                CurrentAmount = 85000m,
                CreatedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
                TargetDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(4)),
                ProgressPercent = 42.5,
                Userid = testUser.Id
            };

            await _context.Savings.AddAsync(saving);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ База данных успешно инициализирована с тестовыми данными!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при инициализации БД: {ex.Message}");
            throw;
        }
    }
}
