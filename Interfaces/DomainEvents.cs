using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public abstract class DomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    public class ExpenseAddedDomainEvent : DomainEvent
    {
        public int ExpenseId { get; init; }
        public int WalletId { get; init; }
        public decimal Amount { get; init; }
        public int UserId { get; init; }
    }

    public class LowBalanceDetectedDomainEvent : DomainEvent
    {
        public int WalletId { get; init; }
        public int UserId { get; init; }
        public decimal CurrentBalance { get; init; }
        public decimal Threshold { get; init; }
    }

    public class BudgetExceededDomainEvent : DomainEvent
    {
        public int BudgetId { get; init; }
        public int CategoryId { get; init; }
        public decimal ExceededAmount { get; init; }
    }

    public class SavingGoalReachedDomainEvent : DomainEvent
    {
        public int SavingId { get; init; }
        public int UserId { get; init; }
        public string GoalName { get; init; } = string.Empty;
    }
}
