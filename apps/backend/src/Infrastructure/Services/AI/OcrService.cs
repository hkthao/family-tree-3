using System.Net.Http.Headers;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Files.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

/// <summary>
/// Dịch vụ để gọi dịch vụ OCR Python bên ngoài.
/// </summary>
public class OcrService : IOcrService
{
    private readonly ILogger<OcrService> _logger;
    private readonly HttpClient _httpClient;
    private readonly OcrSettings _ocrSettings; // Changed from N8nSettings

    public OcrService(
        ILogger<OcrService> logger,
        HttpClient httpClient,
        IOptions<OcrSettings> ocrSettings) // Changed from IOptions<N8nSettings>
    {
        _logger = logger;
        _httpClient = httpClient;
        _ocrSettings = ocrSettings.Value; // Changed from _n8nSettings.Value
    }

    /// <summary>
    /// Thực hiện nhận dạng ký tự quang học (OCR) trên một tệp đã cho bằng cách gọi dịch vụ OCR Python.
    /// </summary>
    /// <param name="fileBytes">Nội dung tệp dưới dạng mảng byte.</param>
    /// <param name="contentType">Loại nội dung (ví dụ: image/jpeg, application/pdf).</param>
    /// <param name="fileName">Tên tệp (ví dụ: document.pdf).</param>
    /// <param name="lang">Ngôn ngữ để thực hiện OCR (ví dụ: "eng", "vie", "eng+vie").</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa văn bản được OCR nếu thành công.</returns>
    public async Task<Result<OcrResultDto>> PerformOcrAsync(
        byte[] fileBytes,
        string contentType,
        string fileName,
        string lang = "eng+vie",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Python OCR Service for OCR processing.");

        if (string.IsNullOrEmpty(_ocrSettings.BaseUrl))
        {
            _logger.LogWarning("OCR service BaseUrl is not configured.");
            return Result<OcrResultDto>.Failure("OCR service integration is not configured.", "Configuration");
        }

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName); // "file" is the expected field name by Python service

        // Add language as a form field
        form.Add(new StringContent(lang), "lang");

        var requestUri = _ocrSettings.BaseUrl + "/ocr"; // Changed from _n8nSettings.Ocr.BaseUrl

        try
        {
            var response = await _httpClient.PostAsync(requestUri, form, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call Python OCR Service. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<OcrResultDto>.Failure($"Failed to perform OCR. Status: {response.StatusCode}. Error: {errorContent}", "ExternalService");
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Successfully received response from Python OCR Service.");

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Python uses snake_case, C# DTO uses PascalCase
                PropertyNameCaseInsensitive = true // This helps map snake_case to PascalCase
            };
            var result = JsonSerializer.Deserialize<OcrResultDto>(jsonContent, options);

            if (result == null || !result.Success)
            {
                _logger.LogWarning("OCR service returned an unsuccessful or empty result. Raw response: {RawResponse}", jsonContent);
                return Result<OcrResultDto>.Failure(result?.Text ?? "OCR service returned an unsuccessful result.", "ExternalService");
            }

            return Result<OcrResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the Python OCR Service.");
            return Result<OcrResultDto>.Failure($"An error occurred while performing OCR: {ex.Message}", "Exception");
        }
    }
}
