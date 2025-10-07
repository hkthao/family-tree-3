using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Application.Families.Specifications;
using backend.Domain.Entities;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandler : IRequestHandler<GetFamiliesQuery, Result<IReadOnlyList<FamilyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetFamiliesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<Result<IReadOnlyList<FamilyListDto>>> Handle(GetFamiliesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Result<IReadOnlyList<FamilyListDto>>.Failure("User is not authenticated.");
        }

        // Get the current user's profile including their associated families
        var currentUserProfile = await _context.UserProfiles
            .WithSpecification(new UserProfileByAuth0IdSpec(_user.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserProfile == null)
        {
            // If user profile doesn't exist, they have no access to any families
            return Result<IReadOnlyList<FamilyListDto>>.Success(new List<FamilyListDto>());
        }

        var query = _context.Families.AsQueryable();

        // Apply user access specification first
        query = query.WithSpecification(new FamilyByUserIdSpec(currentUserProfile.Id));

        // Apply other specifications
        query = query.WithSpecification(new FamilySearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new FamilyOrderingSpecification(request.SortBy, request.SortOrder));
        query = query.WithSpecification(new FamilyPaginationSpecification((request.Page - 1) * request.ItemsPerPage, request.ItemsPerPage));

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var familyList = await query
            .ProjectTo<FamilyListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FamilyListDto>>.Success(familyList);
    }
}
