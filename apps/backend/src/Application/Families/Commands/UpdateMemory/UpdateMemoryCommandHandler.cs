using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Localization;

namespace backend.Application.Memories.Commands.UpdateMemory;

public class UpdateMemoryCommandHandler : IRequestHandler<UpdateMemoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<UpdateMemoryCommandHandler> _localizer;

    public UpdateMemoryCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemoryCommandHandler> localizer)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(UpdateMemoryCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var memory = await _context.Memories.FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

        if (memory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"Memory with ID {request.Id}"), ErrorSources.NotFound);
        }

        _mapper.Map(request, memory); // Map command to existing entity

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
