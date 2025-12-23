using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Interfaces.DTO;

public record CreateBudgetDto
{
    [Required]
    public decimal Amount { get; init; }

    [Required]
    public DateOnly PeriodStart { get; init; }

    [Required]
    public DateOnly PeriodEnd { get; init; }

    [Range(0, 100)]
    public int Needs { get; init; }

    [Range(0, 100)]
    public int Wants { get; init; }

    [Range(0, 100)]
    public int Savings { get; init; }

    public int UserId { get; init; }

}