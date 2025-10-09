using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UserProfiles.Queries.GetUserProfileById;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == request.Id, cancellationToken);

        if (userProfile == null)
        {
            return Result.Failure<UserProfileDto>("User profile not found.", "NotFound");
        }

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result.Success(userProfileDto);
    }
}
