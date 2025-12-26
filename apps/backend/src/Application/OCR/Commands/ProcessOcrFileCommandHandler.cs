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

    public ProcessOcrFileCommandHandler(IOcrService ocrService, ILogger<ProcessOcrFileCommandHandler> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task<Result<OcrResultDto>> Handle(ProcessOcrFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang xử lý ProcessOcrFileCommand cho tệp: {FileName}", request.FileName);

        var ocrResult = await _ocrService.PerformOcrAsync(
            request.FileBytes,
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

        _logger.LogInformation("OCR thành công cho tệp {FileName}.", request.FileName);
        return Result<OcrResultDto>.Success(ocrResult.Value);
    }
}
