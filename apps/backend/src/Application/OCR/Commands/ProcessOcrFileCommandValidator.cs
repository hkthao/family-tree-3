namespace backend.Application.OCR.Commands;

/// <summary>
/// Trình xác thực cho ProcessOcrFileCommand.
/// </summary>
public class ProcessOcrFileCommandValidator : AbstractValidator<ProcessOcrFileCommand>
{
    private readonly long MAX_FILE_SIZE_MB = 10; // Kích thước tệp tối đa 10 MB
    private readonly string[] ALLOWED_CONTENT_TYPES = { "image/jpeg", "image/png", "image/tiff", "application/pdf" };

    public ProcessOcrFileCommandValidator()
    {
        RuleFor(x => x.FileBytes)
            .NotEmpty().WithMessage("Tệp không được để trống.")
            .Must(bytes => bytes != null && bytes.Length > 0).WithMessage("Tệp không được để trống.")
            .Must(bytes => bytes.Length <= MAX_FILE_SIZE_MB * 1024 * 1024)
                .WithMessage($"Kích thước tệp không được vượt quá {MAX_FILE_SIZE_MB} MB.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Loại nội dung không được để trống.")
            .Must(contentType => ALLOWED_CONTENT_TYPES.Contains(contentType))
                .WithMessage($"Loại tệp không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", ALLOWED_CONTENT_TYPES)}.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Tên tệp không được để trống.");

        RuleFor(x => x.Lang)
            .NotEmpty().WithMessage("Ngôn ngữ OCR không được để trống.");
    }
}
