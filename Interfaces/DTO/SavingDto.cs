using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record SavingDto(
        int Id,
        string GoalName,
        decimal TargetAmount,
        decimal CurrentAmount,
        DateOnly CreatedDate,
        DateOnly TargetDate,
        double ProgressPercent);

    public record CreateSavingDto(
        string GoalName,
        decimal TargetAmount,
        DateOnly TargetDate,
        int UserId);
}
