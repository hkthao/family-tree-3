using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Application.OCR.Commands;

/// <summary>
/// Xử lý lệnh để thực hiện OCR trên một tệp đã cho.
/// </summary>
public class ProcessOcrFileCommandHandler : IRequestHandler<ProcessOcrFileCommand, Result<OcrResultDto>>
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<ProcessOcrFileCommandHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory; // NEW

    public ProcessOcrFileCommandHandler(
        IOcrService ocrService,
        ILogger<ProcessOcrFileCommandHandler> logger,
        IHttpClientFactory httpClientFactory) // NEW
    {
        _ocrService = ocrService;
        _logger = logger;
        _httpClientFactory = httpClientFactory; // NEW
    }

    public async Task<Result<OcrResultDto>> Handle(ProcessOcrFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang xử lý ProcessOcrFileCommand cho tệp: {FileName}", request.FileName);

        byte[]? fileBytesToProcess = null;

        if (request.FileBytes != null && request.FileBytes.Length > 0)
        {
            fileBytesToProcess = request.FileBytes;
            _logger.LogInformation("Xử lý tệp trực tiếp từ byte array. Kích thước: {Size} bytes.", fileBytesToProcess.Length);
        }
        else if (!string.IsNullOrEmpty(request.FileUrl))
        {
            _logger.LogInformation("Đang tải tệp từ URL: {FileUrl}", request.FileUrl);
            var downloadResult = await DownloadFileBytesAsync(request.FileUrl, cancellationToken);
            if (!downloadResult.IsSuccess)
            {
                _logger.LogError("Không thể tải tệp từ URL {FileUrl}: {Error}", request.FileUrl, downloadResult.Error);
                return Result<OcrResultDto>.Failure(downloadResult.Error ?? "Không thể tải tệp.", downloadResult.ErrorSource ?? "NetworkError");
            }
            fileBytesToProcess = downloadResult.Value;
            _logger.LogInformation("Đã tải tệp từ URL {FileUrl}. Kích thước: {Size} bytes.", request.FileUrl, fileBytesToProcess?.Length);
        }
        else
        {
            _logger.LogError("ProcessOcrFileCommand không có FileBytes hoặc FileUrl hợp lệ.");
            return Result<OcrResultDto>.Failure("Không có dữ liệu tệp để xử lý OCR.");
        }

        if (fileBytesToProcess == null || fileBytesToProcess.Length == 0)
        {
            _logger.LogError("Dữ liệu tệp để xử lý OCR rỗng hoặc không hợp lệ.");
            return Result<OcrResultDto>.Failure("Dữ liệu tệp để xử lý OCR rỗng hoặc không hợp lệ.");
        }

        var ocrResult = await _ocrService.PerformOcrAsync(
            fileBytesToProcess, // Use the resolved byte array
            request.ContentType,
            request.FileName,
            request.Lang,
            cancellationToken);

        if (!ocrResult.IsSuccess)
        {
            _logger.LogError("OCR thất bại cho tệp {FileName}: {Error}", request.FileName, ocrResult.Error);
            return Result<OcrResultDto>.Failure(ocrResult.Error ?? "OCR thất bại.", ocrResult.ErrorSource ?? "Unknown");
        }

        if (ocrResult.Value == null)
        {
            _logger.LogError("OCR thành công nhưng không có giá trị trả về cho tệp {FileName}.", request.FileName);
            return Result<OcrResultDto>.Failure("OCR thành công nhưng không có giá trị trả về.", "ServiceError");
        }

        // Populate ProcessedBytes
        ocrResult.Value.ProcessedBytes = fileBytesToProcess;

        _logger.LogInformation("OCR thành công cho tệp {FileName}.", request.FileName);
        return Result<OcrResultDto>.Success(ocrResult.Value);
    }

    /// <summary>
    /// Tải nội dung tệp dưới dạng mảng byte từ một URL.
    /// </summary>
    private async Task<Result<byte[]>> DownloadFileBytesAsync(string fileUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var bytes = await httpClient.GetByteArrayAsync(fileUrl, cancellationToken);
            return Result<byte[]>.Success(bytes);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Lỗi HTTP khi tải xuống tệp từ URL: {FileUrl}", fileUrl);
            return Result<byte[]>.Failure($"Lỗi HTTP khi tải xuống tệp từ URL {fileUrl}: {httpEx.Message}", "NetworkError");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải xuống tệp từ URL: {FileUrl}", fileUrl);
            return Result<byte[]>.Failure($"Lỗi khi tải xuống tệp từ URL {fileUrl}: {ex.Message}", "UnknownError");
        }
    }
}
