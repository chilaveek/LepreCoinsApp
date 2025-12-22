// Infrastructure/LC.BLL/Services/GoogleVisionReceiptScanner.cs
namespace LepreCoins.Infrastructure.LC.BLL.Services;

using Google.Cloud.Vision.V1;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

public class GoogleVisionReceiptScanner : IReceiptScannerService
{
    private readonly ImageAnnotatorClient _visionClient;
    private readonly ILogger<GoogleVisionReceiptScanner> _logger;

    public GoogleVisionReceiptScanner(ILogger<GoogleVisionReceiptScanner> logger)
    {
        _logger = logger;
        // Нужно установить переменную среды: GOOGLE_APPLICATION_CREDENTIALS
        _visionClient = ImageAnnotatorClient.Create();
    }

    public async Task<Result<ScannedReceiptDto>> ScanReceiptAsync(Stream imageStream)
    {
        try
        {
            var imageData = new byte[imageStream.Length];
            await imageStream.ReadAsync(imageData, 0, imageData.Length);

            var image = Google.Cloud.Vision.V1.Image.FromBytes(imageData);
            var request = new AnnotateImageRequest
            {
                Image = image,
                Features =
            {
                new Feature { Type = Feature.Types.Type.TextDetection },
                new Feature { Type = Feature.Types.Type.DocumentTextDetection }
            }
            };

            var response = await _visionClient.AnnotateAsync(request);
            var textAnnotations = response.TextAnnotations;

            if (!textAnnotations.Any())
                return Result<ScannedReceiptDto>.Failure("Текст на чеке не найден");

            var fullText = textAnnotations.First().Description;

            return await ParseReceiptTextAsync(fullText, imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при сканировании: {ex.Message}");
            return Result<ScannedReceiptDto>.Failure($"Ошибка при сканировании: {ex.Message}");
        }
    }

    public async Task<Result<ScannedReceiptDto>> ScanReceiptFromFileAsync(string filePath)
    {
        try
        {
            var imageData = await File.ReadAllBytesAsync(filePath);
            var image = Google.Cloud.Vision.V1.Image.FromBytes(imageData);

            var request = new AnnotateImageRequest
            {
                Image = image,
                Features = { new Feature { Type = Feature.Types.Type.TextDetection } }
            };

            var response = await _visionClient.AnnotateAsync(request);
            var fullText = response.TextAnnotations.FirstOrDefault()?.Description ?? "";

            return await ParseReceiptTextAsync(fullText, imageData);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при загрузке файла: {ex.Message}");
            return Result<ScannedReceiptDto>.Failure($"Ошибка при загрузке файла: {ex.Message}");
        }
    }

    public async Task<Result<List<ReceiptItem>>> ExtractItemsAsync(byte[] imageData)
    {
        try
        {
            var image = Google.Cloud.Vision.V1.Image.FromBytes(imageData);
            var request = new AnnotateImageRequest
            {
                Image = image,
                Features = { new Feature { Type = Feature.Types.Type.TextDetection } }
            };

            var response = await _visionClient.AnnotateAsync(request);
            var fullText = response.TextAnnotations.FirstOrDefault()?.Description ?? "";

            var items = ParseReceiptItems(fullText);
            return await Task.FromResult(Result<List<ReceiptItem>>.Success(items));
        }
        catch (Exception ex)
        {
            return Result<List<ReceiptItem>>.Failure($"Ошибка при извлечении предметов: {ex.Message}");
        }
    }

    private async Task<Result<ScannedReceiptDto>> ParseReceiptTextAsync(string text, byte[] imageData)
    {
        try
        {
            var items = ParseReceiptItems(text);
            var totalAmount = ExtractTotal(text);
            var storeName = ExtractStoreName(text);
            var date = ExtractDate(text);

            var receipt = new ScannedReceiptDto(
                date ?? DateTime.Now,
                storeName,
                totalAmount,
                items,
                text);

            return await Task.FromResult(Result<ScannedReceiptDto>.Success(receipt));
        }
        catch (Exception ex)
        {
            return Result<ScannedReceiptDto>.Failure($"Ошибка при обработке текста: {ex.Message}");
        }
    }

    private List<ReceiptItem> ParseReceiptItems(string text)
    {
        var items = new List<ReceiptItem>();

        // Регулярное выражение для поиска товаров и цен
        var pattern = @"^(.+?)\s+(\d+[.,]\d{2})$";
        var regex = new Regex(pattern, RegexOptions.Multiline);

        var matches = regex.Matches(text);
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                // groups[1] – описание, groups[2] – цена
                var description = match.Groups[1].Value.Trim();
                var priceStr = match.Groups[2].Value.Replace(",", ".");

                if (decimal.TryParse(priceStr, out var price))
                {
                    items.Add(new ReceiptItem(description, price, 1));
                }
            }
        }

        return items;
    }

    private decimal? ExtractTotal(string text)
    {
        // Поиск итоговой суммы
        var patterns = new[]
        {
        @"ИТОГО[:\s]+(\d+[.,]\d{2})",
        @"TOTAL[:\s]+(\d+[.,]\d{2})",
        @"Сумма[:\s]+(\d+[.,]\d{2})"
    };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var value = match.Groups[1].Value.Replace(",", ".");
                if (decimal.TryParse(value, out var total))
                {
                    return total;
                }
            }
        }

        return null;
    }

    private string? ExtractStoreName(string text)
    {
        // Обычно название магазина находится в начале чека
        var lines = text.Split('\n');
        return lines.FirstOrDefault()?.Trim();
    }

    private DateTime? ExtractDate(string text)
    {
        // Поиск даты в формате DD.MM.YYYY или DD/MM/YYYY
        var pattern = @"(\d{1,2}[./]\d{1,2}[./]\d{4})";
        var match = Regex.Match(text, pattern);

        if (match.Success && DateTime.TryParseExact(
                match.Groups[1].Value,
                new[] { "dd.MM.yyyy", "dd/MM/yyyy" },
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
        {
            return date;
        }

        return null;
        }
}
