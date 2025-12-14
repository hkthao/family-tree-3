using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Prompts.Commands.DeletePrompt;

public class DeletePromptCommandHandler : IRequestHandler<DeletePromptCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeletePromptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeletePromptCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Prompts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound("Prompt not found.", "NotFound");
        }

        _context.Prompts.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
