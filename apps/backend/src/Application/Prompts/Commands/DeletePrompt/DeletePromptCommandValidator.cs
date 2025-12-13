using backend.Application.Common.Interfaces;

namespace backend.Application.Prompts.Commands.DeletePrompt;

public class DeletePromptCommandValidator : AbstractValidator<DeletePromptCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePromptCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotNull().WithMessage("Id is required.")
            .MustAsync(PromptExists).WithMessage("Prompt not found.");
    }

    private async Task<bool> PromptExists(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Prompts.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
