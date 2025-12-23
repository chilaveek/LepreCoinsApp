using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public int? Familyid { get; set; }

    public int? Budgetid { get; set; }

    public virtual Budget? Budget { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual Family? Family { get; set; }

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<Saving> Savings { get; set; } = new List<Saving>();

    public virtual ICollection<TransferToSaving> TransferToSavings { get; set; } = new List<TransferToSaving>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}