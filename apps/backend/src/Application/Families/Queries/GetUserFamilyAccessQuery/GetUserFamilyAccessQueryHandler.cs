using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Families.Queries.GetUserFamilyAccessQuery;

public class GetUserFamilyAccessQueryHandler : IRequestHandler<GetUserFamilyAccessQuery, Result<List<FamilyAccessDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetUserFamilyAccessQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilyAccessDto>>> Handle(GetUserFamilyAccessQuery request, CancellationToken cancellationToken)
    {
        var familyAccessList = await _context.FamilyUsers
            .Where(fu => fu.UserId == _currentUser.UserId && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Viewer))
            .Select(fu => new FamilyAccessDto
            {
                FamilyId = fu.FamilyId,
                Role = fu.Role
            })
            .ToListAsync(cancellationToken);

        return Result<List<FamilyAccessDto>>.Success(familyAccessList);
    }
}
