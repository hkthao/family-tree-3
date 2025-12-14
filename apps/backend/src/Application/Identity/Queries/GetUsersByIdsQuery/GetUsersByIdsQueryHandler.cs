using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.Queries; // Added
using backend.Application.Identity.Specifications; // Updated

namespace backend.Application.Identity.Queries;

public class GetUsersByIdsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetUsersByIdsQuery, Result<List<UserDto>>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<List<UserDto>>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        var usersSpec = new UsersByIdsSpec(request.Ids);
        var users = await _context.Users
            .WithSpecification(usersSpec)
            .Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                AuthProviderId = u.AuthProviderId,
                Email = u.Email,
                Name = u.Profile!.Name
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserDto>>.Success(users);
    }
}
