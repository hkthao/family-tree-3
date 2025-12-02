using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.FamilyDicts.Commands.DeleteFamilyDict;

public class DeleteFamilyDictCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDateTime dateTime, IAuthorizationService authorizationService) : IRequestHandler<DeleteFamilyDictCommand>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task Handle(DeleteFamilyDictCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            throw new ForbiddenAccessException("Chỉ quản trị viên mới được phép xóa FamilyDict.");
        }

        var entity = await _context.FamilyDicts
            .Where(f => f.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(FamilyDict), request.Id.ToString());
        }

        entity.IsDeleted = true;
        entity.DeletedBy = _currentUser.UserId.ToString();
        entity.DeletedDate = _dateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
