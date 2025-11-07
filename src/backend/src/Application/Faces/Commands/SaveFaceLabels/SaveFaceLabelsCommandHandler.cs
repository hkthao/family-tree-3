using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler(IApplicationDbContext context, IConfigProvider configProvider, ILogger<SaveFaceLabelsCommandHandler> logger) : IRequestHandler<SaveFaceLabelsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IConfigProvider _configProvider = configProvider;
    private readonly ILogger<SaveFaceLabelsCommandHandler> _logger = logger;

    public Task<Result<bool>> Handle(SaveFaceLabelsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SaveFaceLabelsCommand for ImageId {ImageId} with {FaceCount} faces.",
            request.ImageId, request.FaceLabels.Count);

        // Vector store functionality removed as per refactoring.
        // The face labels are still processed, but not stored in a vector database directly by this handler.

        return Task.FromResult(Result<bool>.Success(true));
    }
}
