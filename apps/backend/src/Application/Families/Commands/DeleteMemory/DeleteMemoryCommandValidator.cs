namespace backend.Application.Memories.Commands.DeleteMemory;

public class DeleteMemoryCommandValidator : AbstractValidator<DeleteMemoryCommand>
{
    public DeleteMemoryCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Memory ID is required.");
    }
}
