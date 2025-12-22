using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class Expense
{
    public int Id { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? Sum { get; set; }

    public string? Description { get; set; }

    public string? ExpenseType { get; set; }

    public int? Categoryid { get; set; }

    public int? Userid { get; set; }

    public int? Walletid { get; set; }

    public virtual ExpenseCategory? Category { get; set; }

    public virtual User? User { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
