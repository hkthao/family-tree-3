using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Common.Models;
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
        var query = _context.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .AsQueryable();

        if (request.FamilyId.HasValue)
        {
            query = query.Where(r => r.SourceMember.FamilyId == request.FamilyId.Value);
        }

        if (request.SourceMemberId.HasValue)
        {
            query = query.Where(r => r.SourceMemberId == request.SourceMemberId.Value);
        }

        if (request.TargetMemberId.HasValue)
        {
            query = query.Where(r => r.TargetMemberId == request.TargetMemberId.Value);
        }

        if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<RelationshipType>(request.Type, true, out var relationshipType))
        {
            query = query.Where(r => r.Type == relationshipType);
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            bool isDescending = request.SortOrder?.ToLower() == "desc";
            query = request.SortBy.ToLower() switch
            {
                "sourcememberfullname" => isDescending ? query.OrderByDescending(r => r.SourceMember!.FullName) : query.OrderBy(r => r.SourceMember!.FullName),
                "targetmemberfullname" => isDescending ? query.OrderByDescending(r => r.TargetMember!.FullName) : query.OrderBy(r => r.TargetMember!.FullName),
                "type" => isDescending ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type),
                _ => query.OrderBy(r => r.Id) // Default sort
            };
        }
        else
        {
            query = query.OrderBy(r => r.Id); // Default sort
        }

        var paginatedList = await query
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<RelationshipListDto>>.Success(paginatedList);
    }
}
