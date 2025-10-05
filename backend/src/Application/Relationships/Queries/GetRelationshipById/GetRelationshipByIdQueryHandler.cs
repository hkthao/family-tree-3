using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Queries.GetRelationshipById;

public class GetRelationshipByIdQueryHandler : IRequestHandler<GetRelationshipByIdQuery, Result<RelationshipDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRelationshipByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RelationshipDto>> Handle(GetRelationshipByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new RelationshipByIdSpecification(request.Id);

        var relationshipDto = await _context.Relationships.AsQueryable().WithSpecification(spec)
            .ProjectTo<RelationshipDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (relationshipDto == null)
        {
            return Result<RelationshipDto>.Failure($"Relationship with ID {request.Id} not found.");
        }

        return Result<RelationshipDto>.Success(relationshipDto);
    }
}
