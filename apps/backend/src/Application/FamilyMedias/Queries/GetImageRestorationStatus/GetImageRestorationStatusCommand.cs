using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyMedias.Queries.GetImageRestorationStatus;

public record GetImageRestorationStatusCommand(Guid JobId) : IRequest<Result<ImageRestorationJobStatusDto>>;

public class GetImageRestorationStatusCommandHandler(IImageRestorationService imageRestorationService) : IRequestHandler<GetImageRestorationStatusCommand, Result<ImageRestorationJobStatusDto>>
{
    private readonly IImageRestorationService _imageRestorationService = imageRestorationService;

    public async Task<Result<ImageRestorationJobStatusDto>> Handle(GetImageRestorationStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await _imageRestorationService.GetJobStatusAsync(request.JobId);
        return result;
    }
}
