namespace backend.Application.Files.UploadFile;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(v => v.FileStream)
            .NotNull().WithMessage("FileStream cannot be null.");

        RuleFor(v => v.FileName)
            .NotNull().WithMessage("FileName cannot be null.")
            .NotEmpty().WithMessage("FileName cannot be empty.");

        RuleFor(v => v.ContentType)
            .NotNull().WithMessage("ContentType cannot be null.")
            .NotEmpty().WithMessage("ContentType cannot be empty.");

        RuleFor(v => v.Length)
            .GreaterThan(0).WithMessage("File length must be greater than 0.");
    }
}
