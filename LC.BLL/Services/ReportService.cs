using ApplicationCore.Interfaces;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                var reportDataResult = await GetReportDataAsync(userId, dateRange);
                if (!reportDataResult.IsSuccess || reportDataResult.Data == null)
                    return Result<byte[]>.Failure("Ошибка получения данных");

                var reportData = reportDataResult.Data;

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                string fontPath = @"C:\Windows\Fonts\arial.ttf";
                PdfFont font;

                if (File.Exists(fontPath))
                {
                    font = PdfFontFactory.CreateFont(fontPath, "Identity-H", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }
                else
                {
                    font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                }

                document.SetFont(font);

                document.Add(new Paragraph("ОТЧЕТ О БЮДЖЕТЕ")
                    .SetFontSize(22)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph($"Период: {dateRange.StartDate:dd.MM.yyyy} - {dateRange.EndDate:dd.MM.yyyy}")
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\nФИНАНСОВАЯ СВОДКА").SetFontSize(16));

                var summaryTable = new Table(2).UseAllAvailableWidth();
                summaryTable.AddCell("Общий доход");
                summaryTable.AddCell(reportData.TotalIncome.ToString("C"));
                summaryTable.AddCell("Общие расходы");
                summaryTable.AddCell(reportData.TotalExpenses.ToString("C"));
                summaryTable.AddCell("Итоговый баланс");
                summaryTable.AddCell(reportData.NetCash.ToString("C"));
                document.Add(summaryTable);

                document.Add(new Paragraph("\nДЕТАЛИЗАЦИЯ ОПЕРАЦИЙ").SetFontSize(16));

                var table = new Table(4).UseAllAvailableWidth();
                table.AddHeaderCell("Дата");
                table.AddHeaderCell("Описание");
                table.AddHeaderCell("Сумма");
                table.AddHeaderCell("Тип");

                foreach (var t in reportData.Transactions)
                {
                    table.AddCell(t.Date.ToString("dd.MM.yyyy"));
                    table.AddCell(t.Description ?? "—");
                    table.AddCell(t.Amount.ToString("N2"));
                    table.AddCell(t.Type ?? "—");
                }
                document.Add(table);

                document.Close();
                return Result<byte[]>.Success(stream.ToArray());
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"Ошибка PDF: {ex.Message}");
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
                decimal totalExpenses = 0;
                decimal totalIncome = 0;

                foreach (var e in expenses)
                {
                    decimal amount = e.Sum ?? 0;
                    totalExpenses += amount;
                    transactions.Add(new TransactionDto(
                        e.Id,
                        e.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                        amount,
                        "Расход",
                        e.Description ?? "",
                        "Expense"));
                }

                foreach (var i in incomes)
                {
                    decimal amount = i.Sum ?? 0;
                    totalIncome += amount;
                    transactions.Add(new TransactionDto(
                        i.Id,
                        i.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                        amount,
                        "Доход",
                        i.Description ?? "",
                        "Income"));
                }

                var report = new ReportDataDto(
                    userId,
                    DateTime.Now,
                    transactions.OrderByDescending(t => t.Date).ToList(),
                    null,
                    totalIncome,
                    totalExpenses,
                    totalIncome - totalExpenses
                );

                return Result<ReportDataDto>.Success(report);
            }
            catch (Exception ex)
            {
                return Result<ReportDataDto>.Failure(ex.Message);
            }
        }
    }
}