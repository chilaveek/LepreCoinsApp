using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class TransferToSaving
{
    public int Id { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? Sum { get; set; }

    public string? Description { get; set; }

    public int? Savingsid { get; set; }

    public int? Userid { get; set; }

    public bool? Isdeposit { get; set; }

    public int? Column { get; set; }

    public virtual Saving? Savings { get; set; }

    public virtual User? User { get; set; }
}
