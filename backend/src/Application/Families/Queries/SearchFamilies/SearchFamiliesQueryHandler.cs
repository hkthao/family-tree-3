using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;

namespace backend.Application.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandler : IRequestHandler<SearchFamiliesQuery, PaginatedList<FamilyDto>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMapper _mapper;

    public SearchFamiliesQueryHandler(IFamilyRepository familyRepository, IMapper mapper)
    {
        _familyRepository = familyRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FamilyDto>> Handle(SearchFamiliesQuery request, CancellationToken cancellationToken)
    {
        var query = (await _familyRepository.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(f => f.Name.Contains(request.Keyword) || (f.Description != null && f.Description.Contains(request.Keyword)));
        }

        return await query
            .ProjectTo<FamilyDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
