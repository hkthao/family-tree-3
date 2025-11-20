using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relationships.Queries.GetPublicRelationshipsByFamilyId;

public class GetPublicRelationshipsByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPublicRelationshipsByFamilyIdQuery, Result<List<RelationshipListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<RelationshipListDto>>> Handle(GetPublicRelationshipsByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Verify if the family exists and is public
        var family = await _context.Families
            .AsNoTracking()
            .WithSpecification(new FamilyByIdSpecification(request.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<List<RelationshipListDto>>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        if (family.Visibility != FamilyVisibility.Public.ToString())
        {
            return Result<List<RelationshipListDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Retrieve relationships for that family
        var relationships = await _context.Relationships
            .AsNoTracking()
            .WithSpecification(new RelationshipByFamilyIdSpecification(request.FamilyId))
            .ProjectTo<RelationshipListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<RelationshipListDto>>.Success(relationships);
    }
}
