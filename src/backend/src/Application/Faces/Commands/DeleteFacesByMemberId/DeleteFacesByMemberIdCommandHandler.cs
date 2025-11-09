using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DeleteFacesByMemberId;

public class DeleteFacesByMemberIdCommandHandler : IRequestHandler<DeleteFacesByMemberIdCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IN8nService _n8nService;
    private readonly ILogger<DeleteFacesByMemberIdCommandHandler> _logger;

    public DeleteFacesByMemberIdCommandHandler(IApplicationDbContext context, IN8nService n8nService, ILogger<DeleteFacesByMemberIdCommandHandler> logger)
    {
        _context = context;
        _n8nService = n8nService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteFacesByMemberIdCommand request, CancellationToken cancellationToken)
    {
        var spec = new FacesByMemberIdSpecification(request.MemberId);
        var faces = await _context.Faces.WithSpecification(spec).ToListAsync(cancellationToken);

        if (faces == null || !faces.Any())
        {
            return Result<Unit>.Success(Unit.Value);
        }

        foreach (var face in faces)
        {
            var embeddingDto = new EmbeddingWebhookDto
            {
                EntityType = "Face",
                EntityId = face.Id.ToString(),
                ActionType = "DeleteFaceEmbedding",
                Description = $"Delete face embedding for FaceId {face.Id}"
            };

            var n8nResult = await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);
            if (!n8nResult.IsSuccess)
            {
                _logger.LogError("Failed to delete face embedding for FaceId {FaceId} in n8n: {Error}", face.Id, n8nResult.Error);
                // Continue deleting other faces even if one fails
            }
        }

        _context.Faces.RemoveRange(faces);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
