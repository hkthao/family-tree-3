using FluentValidation;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace backend.Application.Memories.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandValidator : AbstractValidator<AnalyzePhotoCommand>
{
    private const long MaxFileSize = 15 * 1024 * 1024; // 15 MB
    private readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/heic" };

    public AnalyzePhotoCommandValidator()
    {
        RuleFor(v => v.File)
            .NotNull().WithMessage("File is required.")
            .Must(BeAValidFileSize).WithMessage($"File size exceeds {MaxFileSize / (1024 * 1024)} MB limit.")
            .Must(BeAValidFileType).WithMessage("Unsupported file type. Only JPG, PNG, and HEIC are allowed.");
    }

    private bool BeAValidFileSize(IFormFile? file)
    {
        return file != null && file.Length > 0 && file.Length <= MaxFileSize;
    }

    private bool BeAValidFileType(IFormFile? file)
    {
        return file != null && AllowedContentTypes.Contains(file.ContentType.ToLower());
    }
}
