using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        // Get all EventIds where the member is related
        var eventIds = await _context.EventMembers
            .Where(em => em.MemberId == request.MemberId)
            .Select(em => em.EventId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get LocationLinks related to these events
        var eventLocationLinks = await _context.LocationLinks
            .Include(ll => ll.Location) // Include Location for DTO mapping
            .Where(ll => eventIds.Contains(Guid.Parse(ll.RefId)) && ll.RefType == RefType.Event)
            .ProjectTo<LocationLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Optionally, if a member can also be linked to a family directly and that family has location links
        // We need to clarify if Member has a direct link to Family or if it's via FamilyUser
        // For now, let's assume direct family links are not the primary focus for a member's location links.

        return Result<List<LocationLinkDto>>.Success(eventLocationLinks);
    }
}
