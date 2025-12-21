using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR; // Ensure MediatR is included

namespace backend.Application.Prompts.Commands.CreatePrompt;

public class CreatePromptCommandHandler : IRequestHandler<CreatePromptCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public CreatePromptCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result<Guid>> Handle(CreatePromptCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin mới có thể tạo prompt
        if (!_authorizationService.IsAdmin())
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = new Prompt
        {
            Code = request.Code,
            Title = request.Title,
            Content = request.Content,
            Description = request.Description
        };

        _context.Prompts.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
