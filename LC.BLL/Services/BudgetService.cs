using ApplicationCore.Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using LC.BLL.Session;
using LepreCoins.Infrastructure.LC.DAL;
using LepreCoins.Models;
using Interfaces;
namespace LC.BLL.Services;

public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;

    public BudgetService(IUnitOfWork context)
    {
        _unitOfWork = context;
    }

    public async Task<bool> CreateBudgetAsync(CreateBudgetDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null) return false;

            Budget budgetEntity;

            if (user.Budgetid.HasValue && user.Budgetid > 0)
            {
                budgetEntity = await _unitOfWork.BudgetRepository.GetByIdAsync(user.Budgetid.Value);

                if (budgetEntity != null)
                {
                    budgetEntity.EstablishedAmount = dto.Amount;
                    budgetEntity.PeriodStart = dto.PeriodStart;
                    budgetEntity.PeriodEnd = dto.PeriodEnd;
                    budgetEntity.NeedsPercentage = dto.Needs;
                    budgetEntity.WantsPercentage = dto.Wants;
                    budgetEntity.SavingsPercentage = dto.Savings;

                    await _unitOfWork.BudgetRepository.UpdateAsync(budgetEntity);
                }
                else
                {
                    budgetEntity = CreateNewBudgetEntity(dto);
                    await _unitOfWork.BudgetRepository.AddAsync(budgetEntity);
                }
            }
            else
            {
                budgetEntity = CreateNewBudgetEntity(dto);
                await _unitOfWork.BudgetRepository.AddAsync(budgetEntity);
            }

            await _unitOfWork.SaveChangesAsync();

            if (user.Budgetid != budgetEntity.Id)
            {
                user.Budgetid = budgetEntity.Id;
                await _unitOfWork.SaveChangesAsync();

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
            var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(id);
            return budget;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении бюджета: {ex.Message}");
            return null;
        }
    }
    public async Task<Result> ResetBudgetPeriodAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null) return Result.Failure("Пользователь не найден");

            if (user.Budgetid == null)
                return Result.Failure("У пользователя не настроен бюджет");

            var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(user.Budgetid.Value);
            if (budget == null)
                return Result.Failure("Бюджет не найден в базе данных");

            budget.CurrentExpenses = 0;
            budget.SpentNeeds = 0;
            budget.SpentWants = 0;
            budget.SpentSavings = 0;

            await _unitOfWork.BudgetRepository.UpdateAsync(budget);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при сбросе периода: {ex.Message}");
        }
    }
}