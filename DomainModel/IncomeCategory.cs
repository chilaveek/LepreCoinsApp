using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class IncomeCategory
{
    public int Id { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();
}
