using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyDicts.Commands.DeleteFamilyDict;

public class DeleteFamilyDictCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDateTime dateTime, IAuthorizationService authorizationService) : IRequestHandler<DeleteFamilyDictCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result> Handle(DeleteFamilyDictCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            return Result.Forbidden("Chỉ quản trị viên mới được phép xóa FamilyDict.");
        }

        var entity = await _context.FamilyDicts
            .Where(f => f.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result.NotFound($"FamilyDict with ID {request.Id} not found.");
        }

        entity.IsDeleted = true;
        entity.DeletedBy = _currentUser.UserId.ToString();
        entity.DeletedDate = _dateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
