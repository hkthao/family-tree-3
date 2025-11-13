namespace backend.Application.Files.DeleteFile;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(v => v.FileId)
            .NotEmpty().WithMessage("FileId cannot be empty.");
    }
}
