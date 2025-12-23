using ApplicationCore.Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using LC.BLL.Session;
using LepreCoins.Models;

namespace LC.BLL.Services;

public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _context;

    public BudgetService(IUnitOfWork context)
    {
        _context = context;
    }

    public async Task<bool> CreateBudgetAsync(CreateBudgetDto dto)
    {
        try
        {
            var user = await _context.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null) return false;

            Budget budgetEntity;

            if (user.Budgetid.HasValue && user.Budgetid > 0)
            {
                budgetEntity = await _context.BudgetRepository.GetByIdAsync(user.Budgetid.Value);

                if (budgetEntity != null)
                {
                    budgetEntity.EstablishedAmount = dto.Amount;
                    budgetEntity.PeriodStart = dto.PeriodStart;
                    budgetEntity.PeriodEnd = dto.PeriodEnd;
                    budgetEntity.NeedsPercentage = dto.Needs;
                    budgetEntity.WantsPercentage = dto.Wants;
                    budgetEntity.SavingsPercentage = dto.Savings;

                    await _context.BudgetRepository.UpdateAsync(budgetEntity);
                }
                else
                {
                    budgetEntity = CreateNewBudgetEntity(dto);
                    await _context.BudgetRepository.AddAsync(budgetEntity);
                }
            }
            else
            {
                budgetEntity = CreateNewBudgetEntity(dto);
                await _context.BudgetRepository.AddAsync(budgetEntity);
            }

            await _context.SaveChangesAsync();

            if (user.Budgetid != budgetEntity.Id)
            {
                user.Budgetid = budgetEntity.Id;
                await _context.SaveChangesAsync();

                if (Session.Session.CurrentUser != null)
                {
                    Session.Session.CurrentUser.Budgetid = budgetEntity.Id;
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private Budget CreateNewBudgetEntity(CreateBudgetDto dto)
    {
        return new Budget
        {
            EstablishedAmount = dto.Amount,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd,
            NeedsPercentage = dto.Needs,
            WantsPercentage = dto.Wants,
            SavingsPercentage = dto.Savings,
            CurrentExpenses = 0
        };
    }
    public async Task<Budget> GetBudgetByIdAsync(int id)
    {
        try
        {
            var budget = await _context.BudgetRepository.GetByIdAsync(id);
            return budget;
        }
        catch (Exception ex)
        {
            // Здесь можно добавить логгирование ошибки
            Console.WriteLine($"Ошибка при получении бюджета: {ex.Message}");
            return null;
        }
    }
}