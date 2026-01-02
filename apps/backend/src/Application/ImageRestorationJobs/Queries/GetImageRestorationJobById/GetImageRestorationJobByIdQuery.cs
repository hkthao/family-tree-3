using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common; // Added

namespace backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobById;

public record GetImageRestorationJobByIdQuery(string JobId) : IRequest<Result<ImageRestorationJobDto>>;

public class GetImageRestorationJobByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<GetImageRestorationJobByIdQuery, Result<ImageRestorationJobDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ImageRestorationJobDto>> Handle(GetImageRestorationJobByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Result<ImageRestorationJobDto>.Failure("User is not authenticated.", "Authorization");
        }

        var job = await _context.ImageRestorationJobs
            .Where(j => j.UserId == userId && j.JobId == request.JobId)
            .ProjectTo<ImageRestorationJobDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return Result<ImageRestorationJobDto>.NotFound($"Image restoration job with ID '{request.JobId}' not found or you do not have access.", "ImageRestorationJob");
        }

        return Result<ImageRestorationJobDto>.Success(job);
    }
}
