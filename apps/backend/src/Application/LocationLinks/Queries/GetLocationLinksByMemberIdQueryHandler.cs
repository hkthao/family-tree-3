using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.LocationLinks.Queries;

public class GetLocationLinksByMemberIdQueryHandler : IRequestHandler<GetLocationLinksByMemberIdQuery, Result<List<LocationLinkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLocationLinksByMemberIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<LocationLinkDto>>> Handle(GetLocationLinksByMemberIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync(request.MemberId, cancellationToken);
        if (member == null)
        {
            return Result<List<LocationLinkDto>>.Success(new List<LocationLinkDto>());
        }

        // 1. Get LocationLinks directly related to the member
        var memberLocationLinks = await _context.LocationLinks
            .Include(ll => ll.Location)
            .Where(ll => ll.RefType == RefType.Member && ll.RefId == request.MemberId.ToString())
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 2. Get EventIds where the member is related
        var eventIds = await _context.EventMembers
            .Where(em => em.MemberId == request.MemberId)
            .Select(em => em.EventId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get LocationLinks related to these events
        var rawEventLocationLinks = await _context.LocationLinks
            .Include(ll => ll.Location)
            .Where(ll => ll.RefType == RefType.Event)
            .ToListAsync(cancellationToken);

        var eventLocationLinks = rawEventLocationLinks
            .Where(ll => Guid.TryParse(ll.RefId, out var parsedGuid) && eventIds.Contains(parsedGuid))
            .AsQueryable() // Convert back to IQueryable for ProjectTo extension method
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .ToList();

        // 3. Get FamilyId of the member
        var familyId = member.FamilyId.ToString();

        // Get LocationLinks related to the member's family
        var familyLocationLinks = await _context.LocationLinks
            .Include(ll => ll.Location)
            .Where(ll => ll.RefType == RefType.Family && ll.RefId == familyId)
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Combine all lists and remove duplicates
        var allLocationLinks = memberLocationLinks
            .Concat(eventLocationLinks)
            .Concat(familyLocationLinks)
            .GroupBy(ll => ll.Id) // Group by Id to remove duplicates
            .Select(g => g.First())
            .ToList();

        return Result<List<LocationLinkDto>>.Success(allLocationLinks);
    }
}
