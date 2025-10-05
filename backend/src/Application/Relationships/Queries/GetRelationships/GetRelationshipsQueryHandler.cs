using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;
using backend.Application.Relationships.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandler : IRequestHandler<GetRelationshipsQuery, Result<PaginatedList<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRelationshipsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<RelationshipListDto>>> Handle(GetRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relationships.AsQueryable();

        if (request.SourceMemberId.HasValue)
        {
            query = query.Where(r => r.SourceMemberId == request.SourceMemberId.Value);
        }

        if (request.TargetMemberId.HasValue)
        {
            query = query.Where(r => r.TargetMemberId == request.TargetMemberId.Value);
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            if (Enum.TryParse<RelationshipType>(request.Type, true, out var relationshipType))
            {
                query = query.Where(r => r.Type == relationshipType);
            }
        }

        // Include related members for full name display
        query = query.Include(r => r.SourceMember).Include(r => r.TargetMember);

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "sourcememberfullname":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.SourceMember!.FullName) : query.OrderBy(r => r.SourceMember!.FullName);
                    break;
                case "targetmemberfullname":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.TargetMember!.FullName) : query.OrderBy(r => r.TargetMember!.FullName);
                    break;
                case "type":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type);
                    break;
                default:
                    query = query.OrderBy(r => r.SourceMember!.FullName); // Default sort
                    break;
            }
        }
        else
        {
            query = query.OrderBy(r => r.SourceMember!.FullName); // Default sort if no sortBy is provided
        }

        var paginatedList = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<RelationshipListDto>>.Success(paginatedList);
    }
}
