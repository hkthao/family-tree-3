namespace backend.Application.AI.Chunk.ProcessFile;

/// <summary>
/// Trình xác thực cho ProcessFileCommand.
/// </summary>
public class ProcessFileCommandValidator : AbstractValidator<ProcessFileCommand>
{
    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp ProcessFileCommandValidator và định nghĩa các quy tắc xác thực.
    /// </summary>
    public ProcessFileCommandValidator()
    {
        RuleFor(v => v.FileStream)
            .NotNull().WithMessage("FileStream cannot be null.");

        RuleFor(v => v.FileName)
            .NotNull().WithMessage("FileName cannot be null.")
            .NotEmpty().WithMessage("FileName cannot be empty.");

        RuleFor(v => v.FileId)
            .NotNull().WithMessage("FileId cannot be null.")
            .NotEmpty().WithMessage("FileId cannot be empty.");

        RuleFor(v => v.Category)
            .NotNull().WithMessage("Category cannot be null.")
            .NotEmpty().WithMessage("Category cannot be empty.");

        RuleFor(v => v.CreatedBy)
            .NotNull().WithMessage("CreatedBy cannot be null.")
            .NotEmpty().WithMessage("CreatedBy cannot be empty.");
    }
}
