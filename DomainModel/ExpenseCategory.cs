using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class ExpenseCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? CategoryType { get; set; }

    public string? Attribute { get; set; }

    public int? Budgetid { get; set; }

    public virtual Budget? Budget { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
