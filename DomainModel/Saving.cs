using System;
using System.Collections.Generic;

namespace LepreCoins.Models;

public partial class Saving
{
    public int Id { get; set; }

    public string? GoalName { get; set; }

    public decimal? TargetAmount { get; set; }

    public decimal? CurrentAmount { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? TargetDate { get; set; }

    public double? ProgressPercent { get; set; }

    public int? Userid { get; set; }

    public virtual ICollection<TransferToSaving> TransferToSavings { get; set; } = new List<TransferToSaving>();

    public virtual User? User { get; set; }
}