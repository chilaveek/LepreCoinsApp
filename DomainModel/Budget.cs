public class Budget
{
    public int Id { get; set; }
    public decimal? EstablishedAmount { get; set; }
    public decimal? CurrentExpenses { get; set; }

    // Новые свойства
    public decimal? SpentNeeds { get; set; }
    public decimal? SpentWants { get; set; }
    public decimal? SpentSavings { get; set; }

    public int NeedsPercentage { get; set; }
    public int WantsPercentage { get; set; }
    public int SavingsPercentage { get; set; }

    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
}