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

    public virtual ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
