using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.DTO;
namespace Interfaces.Service
{
    public interface IReportService
    {
        Task<Result<byte[]>> GeneratePdfReportAsync(int userId, DateRange dateRange);
        Task<Result<byte[]>> GenerateBudgetReportAsync(int budgetId);
        Task<Result<ReportDataDto>> GetReportDataAsync(int userId, DateRange dateRange);
    }
}
