using System;
using System.Collections.Generic;

namespace Interfaces.DTO;

public record BudgetDto(
    int Id,
    decimal? EstablishedAmount,
    decimal? CurrentExpenses,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,

    // Наши 3 идеальных показателя
    int MandatoryPercent,
    int OptionalPercent,
    int SavingsPercent
);
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