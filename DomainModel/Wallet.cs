using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class Wallet
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Balance { get; set; }

    public string? Currency { get; set; }

    public int? Userid { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual User? User { get; set; }
}
