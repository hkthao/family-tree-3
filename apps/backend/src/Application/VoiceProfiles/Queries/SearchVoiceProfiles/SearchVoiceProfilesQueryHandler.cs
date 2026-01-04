using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.VoiceProfiles.Queries.SearchVoiceProfiles;

public class SearchVoiceProfilesQueryHandler : IRequestHandler<SearchVoiceProfilesQuery, PaginatedList<VoiceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchVoiceProfilesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<VoiceProfileDto>> Handle(SearchVoiceProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VoiceProfiles
            .Include(vp => vp.Member) // Include Member to filter by FamilyId
            .Where(vp => vp.Member.FamilyId == request.FamilyId && !vp.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchTermLower = request.SearchQuery.ToLower();
            query = query.Where(vp => vp.Label.ToLower().Contains(searchTermLower)); // Changed Name to Label
        }

        if (request.MemberId.HasValue)
        {
            query = query.Where(vp => vp.MemberId == request.MemberId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(vp => vp.Status == request.Status.Value);
        }

        // Default sorting
        query = query.OrderByDescending(vp => vp.Created);

        var paginatedList = await query
            .ProjectTo<VoiceProfileDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return paginatedList;
    }
}
