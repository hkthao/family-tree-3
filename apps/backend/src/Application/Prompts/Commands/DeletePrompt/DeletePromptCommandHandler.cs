using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR; // Ensure MediatR is included

namespace backend.Application.Prompts.Commands.DeletePrompt;

public class DeletePromptCommandHandler : IRequestHandler<DeletePromptCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public DeletePromptCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(DeletePromptCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin mới có thể xóa prompt
        if (!_authorizationService.IsAdmin())
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

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
