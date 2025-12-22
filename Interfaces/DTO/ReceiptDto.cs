using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record ScannedReceiptDto(
        DateTime Date,
        string? Store,
        decimal? TotalAmount,
        List<ReceiptItem> Items,
        string? RawText);

    public record ReceiptItem(
        string Description,
        decimal? Price,
        int? Quantity);
}
