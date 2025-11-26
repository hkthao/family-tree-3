namespace backend.Application.Files.UploadFile;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(v => v.ImageData)
            .NotNull().WithMessage("Image data cannot be null.")
            .Must(data => data != null && data.Length > 0).WithMessage("Image data cannot be empty.");

        RuleFor(v => v.FileName)
            .NotNull().WithMessage("File name cannot be null.")
            .NotEmpty().WithMessage("File name cannot be empty.");

        RuleFor(v => v.Cloud)
            .NotNull().WithMessage("Cloud service name cannot be null.")
            .NotEmpty().WithMessage("Cloud service name cannot be empty.");

        RuleFor(v => v.Folder)
            .NotNull().WithMessage("Folder name cannot be null.")
            .NotEmpty().WithMessage("Folder name cannot be empty.");
    }
}
