using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Application.Common.Models.ImageRestoration; // Added

namespace backend.Application.FamilyMedias.Commands.StartImageRestoration;

public record StartImageRestorationCommand(string ImageUrl) : IRequest<Result<StartImageRestorationResponseDto>>;

public class StartImageRestorationCommandHandler(
    IImageRestorationService imageRestorationService,
    IApplicationDbContext context,
    ICurrentUser currentUser
) : IRequestHandler<StartImageRestorationCommand, Result<StartImageRestorationResponseDto>>
{
    private readonly IImageRestorationService _imageRestorationService = imageRestorationService;
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<StartImageRestorationResponseDto>> Handle(StartImageRestorationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId.ToString(); // Convert Guid to string
        if (string.IsNullOrEmpty(userId))
        {
            return Result<StartImageRestorationResponseDto>.Failure("User is not authenticated.", "Authorization");
        }

        // 1. Create and save initial job record
        var job = new ImageRestorationJob
        {
            OriginalImageUrl = request.ImageUrl,
            UserId = userId,
            Status = RestorationStatus.Processing, // Initial status
            JobId = Guid.NewGuid().ToString(), // Temporary local job ID, will be updated by external service
        };

        _context.ImageRestorationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Call external service
        var serviceResult = await _imageRestorationService.StartRestorationAsync(request.ImageUrl, cancellationToken);

        // 3. Update job record based on service result
        if (serviceResult.IsSuccess)
        {
            job.JobId = serviceResult.Value!.JobId.ToString(); // Update with actual job ID from external service and convert to string
            job.Status = serviceResult.Value.Status;
            job.RestoredImageUrl = serviceResult.Value.OriginalUrl; // 'OriginalUrl' from serviceResult is likely the restored URL after processing.
        }
        else
        {
            job.Status = RestorationStatus.Failed;
            job.ErrorMessage = serviceResult.Error;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Return the service result, which might contain the external job ID and status
        return serviceResult;
    }
}
