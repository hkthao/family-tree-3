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
        // Xác thực rằng chỉ một trong hai FileBytes hoặc FileUrl được cung cấp
        RuleFor(x => x)
            .Must(x => (x.FileBytes != null && x.FileBytes.Length > 0 && string.IsNullOrEmpty(x.FileUrl)) ||
                       (x.FileBytes == null || x.FileBytes.Length == 0 && !string.IsNullOrEmpty(x.FileUrl)))
            .WithMessage("Phải cung cấp FileBytes hoặc FileUrl, không được cả hai hoặc không gì cả.");

        When(x => x.FileBytes != null && x.FileBytes.Length > 0, () =>
        {
            RuleFor(x => x.FileBytes)
                .Must(bytes => bytes != null && bytes.Length <= MAX_FILE_SIZE_MB * 1024 * 1024)
                .WithMessage($"Kích thước tệp không được vượt quá {MAX_FILE_SIZE_MB} MB.");
        });

        When(x => !string.IsNullOrEmpty(x.FileUrl), () =>
        {
            RuleFor(x => x.FileUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("FileUrl phải là một URL hợp lệ.");
        });

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
