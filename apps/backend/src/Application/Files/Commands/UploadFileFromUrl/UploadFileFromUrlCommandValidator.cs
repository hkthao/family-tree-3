namespace backend.Application.Files.Commands.UploadFileFromUrl;

public class UploadFileFromUrlCommandValidator : AbstractValidator<UploadFileFromUrlCommand>
{
    public UploadFileFromUrlCommandValidator()
    {
        RuleFor(v => v.FileUrl)
            .NotNull().WithMessage("File URL cannot be null.")
            .NotEmpty().WithMessage("File URL cannot be empty.")
            .Must(BeAValidUrl).WithMessage("File URL must be a valid URL.");

        RuleFor(v => v.FileName)
            .NotNull().WithMessage("File name cannot be null.")
            .NotEmpty().WithMessage("File name cannot be empty.");



        RuleFor(v => v.Folder)
            .NotNull().WithMessage("Folder name cannot be null.")
            .NotEmpty().WithMessage("Folder name cannot be empty.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
