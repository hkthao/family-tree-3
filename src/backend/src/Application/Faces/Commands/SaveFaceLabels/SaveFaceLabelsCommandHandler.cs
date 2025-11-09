using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler(IApplicationDbContext context, IConfigProvider configProvider, ILogger<SaveFaceLabelsCommandHandler> logger, IN8nService n8nService) : IRequestHandler<SaveFaceLabelsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IConfigProvider _configProvider = configProvider;
    private readonly ILogger<SaveFaceLabelsCommandHandler> _logger = logger;
    private readonly IN8nService _n8nService = n8nService;

    public async Task<Result<bool>> Handle(SaveFaceLabelsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SaveFaceLabelsCommand for ImageId {ImageId} with {FaceCount} faces.",
            request.ImageId, request.FaceLabels.Count);

        foreach (var faceLabel in request.FaceLabels)
        {
            if (faceLabel.Embedding != null && faceLabel.MemberId.HasValue)
            {
                var embeddingDto = new EmbeddingWebhookDto
                {
                    EntityType = "Face",
                    EntityId = faceLabel.Id,
                    ActionType = "SaveFaceEmbedding",
                    EntityData = new
                    {
                        MemberId = faceLabel.MemberId.Value,
                        Embedding = faceLabel.Embedding
                    },
                    Description = $"Face embedding for FaceId {faceLabel.Id} and MemberId {faceLabel.MemberId.Value}"
                };

                var n8nResult = await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);

                if (!n8nResult.IsSuccess)
                {
                    _logger.LogError("Failed to save face embedding for FaceId {FaceId} to n8n: {Error}", faceLabel.Id, n8nResult.Error);
                    return Result<bool>.Failure($"Failed to save face embedding for FaceId {faceLabel.Id} to n8n: {n8nResult.Error}");
                }
            }
        }

        return Result<bool>.Success(true);
    }
}
