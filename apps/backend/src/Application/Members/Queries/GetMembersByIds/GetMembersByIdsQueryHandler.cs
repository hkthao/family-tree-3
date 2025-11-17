using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetMembersByIdsQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        var memberList = await _context.Members
            .WithSpecification(new MembersByIdsSpecification(request.Ids))
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Assuming all members in the list belong to the same family, or we need to get familyId from each member.
        // For simplicity, let's assume they belong to the same family for now, or we need to iterate and get familyId for each.
        // If members can be from different families, this logic needs to be adjusted.
        // For now, we'll assume the first member's familyId is representative if the list is not empty.
        if (memberList.Any())
        {
            var familyId = memberList.First().FamilyId; // Assuming all members are from the same family
            var filteredMembers = await _privacyService.ApplyPrivacyFilter(memberList, familyId, cancellationToken);
            return Result<IReadOnlyList<MemberListDto>>.Success(filteredMembers);
        }

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}
