using Interfaces.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.Service
{
    public interface IBudgetService
    {
        Task<Result<BudgetDto>> CreateBudgetAsync(CreateBudgetDto dto);
        Task<Result<BudgetDto>> UpdateBudgetAsync(int budgetId, UpdateBudgetDto dto);
        Task<Result<BudgetDto>> GetBudgetAsync(int budgetId);
        Task<Result<BudgetAnalysisDto>> GetBudgetAnalysisAsync(int budgetId);
        Task<Result> CheckLowBalanceAlert(int walletId, decimal threshold);
    }
}
