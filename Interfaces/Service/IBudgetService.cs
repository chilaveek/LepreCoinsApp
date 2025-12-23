using Interfaces.DTO;
using LepreCoins.Models;

namespace Interfaces.Service;

public interface IBudgetService
{
    Task<bool> CreateBudgetAsync(CreateBudgetDto dto);
    Task<Budget> GetBudgetByIdAsync(int id);
}