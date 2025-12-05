using backend.Application.Common.Interfaces;

namespace backend.Application.Prompts.Commands.UpdatePrompt;

public class UpdatePromptCommandValidator : AbstractValidator<UpdatePromptCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePromptCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotNull().WithMessage("Id is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(BeUniqueCode).WithMessage("The specified Code already exists.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }

    public async Task<bool> BeUniqueCode(UpdatePromptCommand model, string code, CancellationToken cancellationToken)
    {
        return await _context.Prompts
            .Where(l => l.Id != model.Id)
            .AllAsync(l => l.Code != code, cancellationToken);
    }
}
