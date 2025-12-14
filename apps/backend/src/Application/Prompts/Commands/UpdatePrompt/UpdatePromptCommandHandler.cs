using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Prompts.Commands.UpdatePrompt;

public class UpdatePromptCommandHandler : IRequestHandler<UpdatePromptCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePromptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePromptCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Prompts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result.NotFound("Prompt not found.", "NotFound");
        }

        entity.Code = request.Code;
        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
