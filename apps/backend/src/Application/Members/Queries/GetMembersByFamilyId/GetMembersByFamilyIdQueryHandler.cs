using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications; // Added missing using directive

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

public class GetMembersByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetMembersByFamilyIdQuery, Result<List<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<MemberListDto>>> Handle(GetMembersByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var members = await _context.Members
            .WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId))
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Apply privacy filter to each member in the list
        var filteredMembers = await _privacyService.ApplyPrivacyFilter(members, request.FamilyId, cancellationToken);

        return Result<List<MemberListDto>>.Success(filteredMembers);
    }
}
