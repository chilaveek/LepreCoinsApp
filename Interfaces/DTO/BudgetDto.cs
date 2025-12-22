using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record BudgetDto(
        int Id,
        decimal EstablishedAmount,
        decimal CurrentExpenses,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        List<CategoryBudgetDto> Categories);

    public record CreateBudgetDto(
        decimal EstablishedAmount,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        List<int> UserIds);

    public record UpdateBudgetDto(
        decimal EstablishedAmount,
        DateOnly PeriodStart,
        DateOnly PeriodEnd);

    public record CategoryBudgetDto(
        int Id,
        string Name,
        decimal Limit,
        decimal CurrentExpenses,
        double PercentageUsed);

    public record BudgetAnalysisDto(
        decimal TotalBudget,
        decimal TotalSpent,
        decimal Remaining,
        double PercentageUsed,
        List<CategoryAnalysisDto> Categories);

    public record CategoryAnalysisDto(
        string Category,
        decimal Limit,
        decimal Spent,
        double Percentage,
        bool IsExceeded);
}
