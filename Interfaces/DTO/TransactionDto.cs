using System;

namespace Interfaces.DTO
{
    public record TransactionDto(
        int Id,
        DateTime Date,
        decimal Amount,
        string Category,
        string Description,
        string Type,
        int WalletId,
        string WalletName);

    public record CreateExpenseDto(
        decimal Sum,
        string Description,
        int CategoryId,
        int UserId,
        int WalletId);

    public record UpdateExpenseDto(
        decimal Sum,
        string Description,
        int CategoryId);

    public record CreateIncomeDto(
        decimal Sum,
        string Description,
        int IncomeCategoryId,
        int UserId,
        int WalletId);

    public record UpdateIncomeDto(
        decimal Sum,
        string Description,
        int IncomeCategoryId);
}