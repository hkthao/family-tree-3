using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByAuth0IdQueryHandler : IRequestHandler<GetUserProfileByAuth0IdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileByAuth0IdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByAuth0IdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Auth0UserId == request.Auth0UserId, cancellationToken);

        if (userProfile == null)
        {
            return Result<UserProfileDto>.Failure("User profile not found.", "NotFound");
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }
}
