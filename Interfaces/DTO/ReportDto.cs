using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record ReportDataDto(
        int UserId,
        DateTime ReportDate,
        List<TransactionDto> Transactions,
        BudgetAnalysisDto? BudgetAnalysis,
        decimal TotalIncome,
        decimal TotalExpenses,
        decimal NetCash);
}
