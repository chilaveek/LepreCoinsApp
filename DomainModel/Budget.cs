using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class Budget
{
    public int Id { get; set; }

    public decimal? EstablishedAmount { get; set; }

    public decimal? CurrentExpenses { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public int NeedsPercentage { get; set; }

    public int WantsPercentage { get; set; }

    public int SavingsPercentage { get; set; }

}
