using FluentValidation;
using System.IO;

namespace backend.Application.Files.Commands.ProcessFile;

public class ProcessFileCommandValidator : AbstractValidator<ProcessFileCommand>
{
    public ProcessFileCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream cannot be null.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name cannot be empty.");

        RuleFor(x => x.FileName)
            .Must(BeAValidFileType).WithMessage("Unsupported file type. Only PDF and TXT are allowed.");

        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID cannot be empty.");

        RuleFor(x => x.FamilyId)
            .MaximumLength(36).WithMessage("Family ID must not exceed 36 characters."); // Assuming GUID format

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category cannot be empty.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy cannot be empty.");
    }

    private bool BeAValidFileType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var allowedExtensions = new[] { ".pdf", ".txt" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(fileExtension);
    }
}
