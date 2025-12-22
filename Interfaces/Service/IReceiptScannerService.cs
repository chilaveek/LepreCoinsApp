using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.DTO;
namespace Interfaces.Service
{
    public interface IReceiptScannerService
    {
        Task<Result<ScannedReceiptDto>> ScanReceiptAsync(Stream imageStream);
        Task<Result<ScannedReceiptDto>> ScanReceiptFromFileAsync(string filePath);
        Task<Result<List<ReceiptItem>>> ExtractItemsAsync(byte[] imageData);
    }
}
