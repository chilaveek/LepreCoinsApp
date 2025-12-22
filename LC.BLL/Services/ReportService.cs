using ApplicationCore.Interfaces;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using System;
using System.Collections.Generic;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace LC.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<byte[]>> GeneratePdfReportAsync(int userId, DateRange dateRange)
        {
            try
            {
                var reportData = await GetReportDataAsync(userId, dateRange);
                if (!reportData.IsSuccess || reportData.Data == null)
                    return Result<byte[]>.Failure("Ошибка при получении данных отчета");

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var writer = new PdfWriter(stream);
                        var pdfDoc = new PdfDocument(writer);
                        var document = new Document(pdfDoc);

                        // Заголовок
                        document.Add(new Paragraph("ОТЧЕТ О БЮДЖЕТЕ")
                            .SetFontSize(24));

                        document.Add(new Paragraph($"Период: {dateRange.StartDate:dd.MM.yyyy} - {dateRange.EndDate:dd.MM.yyyy}")
                            .SetFontSize(12));

                        document.Add(new Paragraph("\n"));

                        // Сводка
                        document.Add(new Paragraph("ФИНАНСОВАЯ СВОДКА")
                            .SetFontSize(16));

                        var summaryTable = new Table(2);
                        summaryTable.AddCell("Общий доход");
                        summaryTable.AddCell(reportData.Data.TotalIncome.ToString("C"));
                        summaryTable.AddCell("Общие расходы");
                        summaryTable.AddCell(reportData.Data.TotalExpenses.ToString("C"));
                        summaryTable.AddCell("Чистый кэш");
                        summaryTable.AddCell(reportData.Data.NetCash.ToString("C"));

                        document.Add(summaryTable);
                        document.Add(new Paragraph("\n"));

                        // Транзакции
                        document.Add(new Paragraph("ТРАНЗАКЦИИ")
                            .SetFontSize(16));

                        var transactionsTable = new Table(4);
                        transactionsTable.AddHeaderCell("Дата");
                        transactionsTable.AddHeaderCell("Описание");
                        transactionsTable.AddHeaderCell("Сумма");
                        transactionsTable.AddHeaderCell("Тип");

                        foreach (var transaction in reportData.Data.Transactions)
                        {
                            transactionsTable.AddCell(transaction.Date.ToString("dd.MM.yyyy"));
                            transactionsTable.AddCell(transaction.Description ?? "");
                            transactionsTable.AddCell(transaction.Amount.ToString("C"));
                            transactionsTable.AddCell(transaction.Type ?? "");
                        }

                        document.Add(transactionsTable);

                        // ✅ КРИТИЧНО: Закрыть document ПЕРЕД тем как читать из stream
                        document.Close();

                        // Теперь можно вернуть byte array
                        return Result<byte[]>.Success(stream.ToArray());
                    }
                    catch (Exception ex)
                    {
                        return Result<byte[]>.Failure($"Ошибка при создании PDF: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"Ошибка при генерации PDF отчета: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> GenerateBudgetReportAsync(int budgetId)
        {
            try
            {
                var budget = await _unitOfWork.BudgetRepository.GetByIdAsync(budgetId);
                if (budget == null)
                    return Result<byte[]>.Failure("Бюджет не найден");

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var writer = new PdfWriter(stream);
                        var pdfDoc = new PdfDocument(writer);
                        var document = new Document(pdfDoc);

                        document.Add(new Paragraph("ОТЧЕТ О БЮДЖЕТЕ")
                            .SetFontSize(24));

                        document.Add(new Paragraph($"Период: {budget.PeriodStart:dd.MM.yyyy} - {budget.PeriodEnd:dd.MM.yyyy}")
                            .SetFontSize(12));

                        document.Add(new Paragraph("\n"));

                        var table = new Table(2);
                        table.AddCell("Установленный лимит");
                        table.AddCell((budget.EstablishedAmount ?? 0).ToString("C"));
                        table.AddCell("Текущие расходы");
                        table.AddCell((budget.CurrentExpenses ?? 0).ToString("C"));
                        table.AddCell("Осталось");
                        table.AddCell(((budget.EstablishedAmount ?? 0) - (budget.CurrentExpenses ?? 0)).ToString("C"));

                        document.Add(table);

                        // ✅ КРИТИЧНО: Закрыть document ПЕРЕД тем как читать из stream
                        document.Close();

                        return Result<byte[]>.Success(stream.ToArray());
                    }
                    catch (Exception ex)
                    {
                        return Result<byte[]>.Failure($"Ошибка при создании PDF бюджета: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"Ошибка при генерации отчета бюджета: {ex.Message}");
            }
        }

        public async Task<Result<ReportDataDto>> GetReportDataAsync(int userId, DateRange dateRange)
        {
            try
            {
                var expenses = await _unitOfWork.ExpenseRepository.FindAsync(e =>
                    e.Userid == userId &&
                    e.Date >= DateOnly.FromDateTime(dateRange.StartDate) &&
                    e.Date <= DateOnly.FromDateTime(dateRange.EndDate));

                var incomes = await _unitOfWork.IncomeRepository.FindAsync(i =>
                    i.Userid == userId &&
                    i.Date >= DateOnly.FromDateTime(dateRange.StartDate) &&
                    i.Date <= DateOnly.FromDateTime(dateRange.EndDate));

                var transactions = new List<TransactionDto>();
                var totalExpenses = 0m;
                var totalIncome = 0m;

                foreach (var expense in expenses)
                {
                    var amount = expense.Sum ?? 0;
                    totalExpenses += amount;
                    transactions.Add(new TransactionDto(
                        expense.Id,
                        expense.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                        amount,
                        "Расход",
                        expense.Description ?? "",
                        "Expense"));
                }

                foreach (var income in incomes)
                {
                    var amount = income.Sum ?? 0;
                    totalIncome += amount;
                    transactions.Add(new TransactionDto(
                        income.Id,
                        income.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                        amount,
                        "Доход",
                        income.Description ?? "",
                        "Income"));
                }

                var reportData = new ReportDataDto(
                    userId,
                    DateTime.Now,
                    transactions.OrderByDescending(t => t.Date).ToList(),
                    null,
                    totalIncome,
                    totalExpenses,
                    totalIncome - totalExpenses);

                return Result<ReportDataDto>.Success(reportData);
            }
            catch (Exception ex)
            {
                return Result<ReportDataDto>.Failure($"Ошибка при получении данных отчета: {ex.Message}");
            }
        }
    }
}