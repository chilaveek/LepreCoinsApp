using System;

namespace Interfaces.DTO;

public record UpdateBudgetDto
{
    public int Id { get; init; }

    public decimal? EstablishedAmount { get; init; }

    public DateOnly? PeriodStart { get; init; }

    public DateOnly? PeriodEnd { get; init; }
    public int MandatoryPercent { get; init; }

    public int OptionalPercent { get; init; }

    public int SavingsPercent { get; init; }
}