using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Search.Queries.Search;

public class SearchQueryHandler : IRequestHandler<SearchQuery, PaginatedList<SearchItem>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public SearchQueryHandler(IFamilyRepository familyRepository, IMemberRepository memberRepository, IEventRepository eventRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SearchItem>> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        var families = await _familyRepository.GetAllAsync();
        var members = await _memberRepository.GetAllAsync();
        var events = await _eventRepository.GetAllAsync();

        var familyQuery = families.AsQueryable()
            .Where(f => f.Name.Contains(request.Keyword) || (f.Description != null && f.Description.Contains(request.Keyword)))
            .Select(f => new SearchItem { Id = f.Id, Type = "Family", Name = f.Name, Description = f.Description });

        var memberQuery = members.AsQueryable()
            .Where(m => m.FirstName.Contains(request.Keyword) || m.LastName.Contains(request.Keyword) || (m.Biography != null && m.Biography.Contains(request.Keyword)))
            .Select(m => new SearchItem { Id = m.Id, Type = "Member", Name = m.FullName, Description = m.Biography });

        var eventQuery = events.AsQueryable()
            .Where(e => e.Name.Contains(request.Keyword) || (e.Description != null && e.Description.Contains(request.Keyword)))
            .Select(e => new SearchItem { Id = e.Id, Type = "Event", Name = e.Name, Description = e.Description });

        var combinedQuery = familyQuery.Union(memberQuery).Union(eventQuery);

        return await PaginatedList<SearchItem>.CreateAsync(combinedQuery.AsNoTracking(), request.PageNumber, request.PageSize);
    }
}
