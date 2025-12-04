using FluentValidation;
using backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandValidator : AbstractValidator<CreatePromptCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePromptCommandValidator(IApplicationDbContext context)
    {
        _context = context;

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

    public async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
    {
        return await _context.Prompts
            .AllAsync(l => l.Code != code, cancellationToken);
    }
}
