using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;
using backend.Application.Relationships.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries.SearchRelationships;

public class SearchRelationshipsQueryHandler : IRequestHandler<SearchRelationshipsQuery, Result<PaginatedList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<RelationshipListDto>>> Handle(SearchRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relationships.AsQueryable();

        // Apply individual specifications
        query = query.WithSpecification(new RelationshipBySourceMemberIdSpecification(request.SourceMemberId));
        query = query.WithSpecification(new RelationshipByTargetMemberIdSpecification(request.TargetMemberId));
        query = query.WithSpecification(new RelationshipByTypeSpecification(request.Type));

        // Include related members for full name display
        query = query.Include(r => r.SourceMember).Include(r => r.TargetMember);

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "sourcememberfullname":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.SourceMember!.LastName).ThenByDescending(r => r.SourceMember!.FirstName) : query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName);
                    break;
                case "targetmemberfullname":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.TargetMember!.LastName).ThenByDescending(r => r.TargetMember!.FirstName) : query.OrderBy(r => r.TargetMember!.LastName).ThenBy(r => r.TargetMember!.FirstName);
                    break;
                case "type":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type);
                    break;
                default:
                    query = query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName); // Default sort
                    break;
            }
        }
        else
        {
            query = query.OrderBy(r => r.SourceMember!.LastName).ThenBy(r => r.SourceMember!.FirstName); // Default sort if no sortBy is provided
        }

        var paginatedList = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<RelationshipListDto>>.Success(paginatedList);
    }
}
