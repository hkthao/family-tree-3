using FluentValidation;

namespace backend.Application.Files.CleanupUnusedFiles;

public class CleanupUnusedFilesCommandValidator : AbstractValidator<CleanupUnusedFilesCommand>
{
    public CleanupUnusedFilesCommandValidator()
    {
        RuleFor(v => v.OlderThan.TotalSeconds)
            .GreaterThan(0).WithMessage("OlderThan must be a positive TimeSpan.");
    }
}
