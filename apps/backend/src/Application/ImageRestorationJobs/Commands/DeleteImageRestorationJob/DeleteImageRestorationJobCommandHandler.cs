using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.ImageRestorationJobs.Commands.DeleteImageRestorationJob;

public class DeleteImageRestorationJobCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<DeleteImageRestorationJobCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<Unit>> Handle(DeleteImageRestorationJobCommand request, CancellationToken cancellationToken)
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

        _context.ImageRestorationJobs.Remove(job);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
