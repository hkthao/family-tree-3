using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Specifications;
using backend.Domain.Entities;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandler(IApplicationDbContext context, ILogger<SaveFaceLabelsCommandHandler> logger, IN8nService n8nService) : IRequestHandler<SaveFaceLabelsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context = context;
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
                var spec = new FaceByIdSpecification(Guid.Parse(faceLabel.Id));
                var face = await _context.Faces.WithSpecification(spec).FirstOrDefaultAsync(cancellationToken);

                if (face == null)
                {
                    face = new Face
                    {
                        Id = Guid.Parse(faceLabel.Id),
                        MemberId = faceLabel.MemberId.Value,
                        Thumbnail = faceLabel.Thumbnail,
                        Embedding = faceLabel.Embedding
                    };
                    await _context.Faces.AddAsync(face, cancellationToken);
                }
                else
                {
                    face.MemberId = faceLabel.MemberId.Value;
                    face.Thumbnail = faceLabel.Thumbnail;
                    face.Embedding = faceLabel.Embedding;
                }

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
                    // Do not fail the whole operation, just log the error
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

