using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common; // Added

namespace backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobs;

public record GetImageRestorationJobsQuery : IRequest<Result<List<ImageRestorationJobDto>>>;

public class GetImageRestorationJobsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<GetImageRestorationJobsQuery, Result<List<ImageRestorationJobDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<ImageRestorationJobDto>>> Handle(GetImageRestorationJobsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Result<List<ImageRestorationJobDto>>.Failure("User is not authenticated.", "Authorization");
        }

        var jobs = await _context.ImageRestorationJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.Created)
            .ProjectTo<ImageRestorationJobDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<ImageRestorationJobDto>>.Success(jobs);
    }
}
