using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.ImageRestorationJobs.Commands.UpdateImageRestorationJob;

public class UpdateImageRestorationJobCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<UpdateImageRestorationJobCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Unit>> Handle(UpdateImageRestorationJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Result<Unit>.Failure("User is not authenticated.", "Authorization");
        }

        var job = await _context.ImageRestorationJobs
            .Where(j => j.UserId == userId && j.JobId == request.JobId)
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return Result<Unit>.NotFound($"Image restoration job with ID '{request.JobId}' not found or you do not have access.", "ImageRestorationJob");
        }

        if (request.Status.HasValue)
        {
            job.Status = request.Status.Value;
        }

        if (request.ErrorMessage != null)
        {
            job.ErrorMessage = request.ErrorMessage;
        }

        if (request.RestoredImageUrl != null)
        {
            job.RestoredImageUrl = request.RestoredImageUrl;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
