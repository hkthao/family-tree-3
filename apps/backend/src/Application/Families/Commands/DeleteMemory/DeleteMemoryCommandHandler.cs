using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Localization;

namespace backend.Application.Memories.Commands.DeleteMemory;

public class DeleteMemoryCommandHandler : IRequestHandler<DeleteMemoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<DeleteMemoryCommandHandler> _localizer;
    private readonly IDateTime _dateTime;

    public DeleteMemoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<DeleteMemoryCommandHandler> localizer, IDateTime dateTime)
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(DeleteMemoryCommand request, CancellationToken cancellationToken)
    {
        var memory = await _context.Memories.Include(m => m.Member).FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

        if (memory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"Memory with ID {request.Id}"), ErrorSources.NotFound);
        }

        // Authorization check
        if (!_authorizationService.CanManageFamily(memory.Member.FamilyId)) // Check if user can manage the family the member belongs to
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        memory.IsDeleted = true; // Soft delete
        memory.DeletedDate = _dateTime.Now; // Set deletion timestamp

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
