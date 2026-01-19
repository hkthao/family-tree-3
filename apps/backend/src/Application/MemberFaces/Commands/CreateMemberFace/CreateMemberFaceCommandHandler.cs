using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Queries.SearchVectorFace;
using backend.Domain.Entities;
using backend.Domain.Events.MemberFaces;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CreateMemberFaceCommandHandler> logger, IMediator mediator) : IRequestHandler<CreateMemberFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<CreateMemberFaceCommandHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    public async Task<Result<Guid>> Handle(CreateMemberFaceCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync([request.MemberId], cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure($"Member with ID {request.MemberId} not found.", ErrorSources.NotFound);
        }
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }
        var searchMemberFaceQuery = new SearchMemberFaceQuery(member.FamilyId)
        {
            Vector = [.. request.Embedding],
            Limit = 1,
            Threshold = 0.95f
        };
        var searchQueryResult = await _mediator.Send(searchMemberFaceQuery, cancellationToken);
        if (searchQueryResult.IsSuccess && searchQueryResult.Value != null && searchQueryResult.Value.Any())
        {
            var foundFace = searchQueryResult.Value.First();
            if (foundFace.MemberId != request.MemberId)
            {
                _logger.LogWarning("Face with embedding found in vector DB for a different MemberId {FoundMemberId} (requested: {RequestedMemberId}). Cannot create new MemberFace.", foundFace.MemberId, request.MemberId);
                return Result<Guid>.Failure($"Face with similar embedding is already associated with another member in vector DB.", ErrorSources.Conflict);
            }
        }
        var entity = new MemberFace
        {
            Id = Guid.NewGuid(),
            MemberId = request.MemberId,
            FaceId = request.FaceId,
            BoundingBox = new BoundingBox
            {
                X = request.BoundingBox.X,
                Y = request.BoundingBox.Y,
                Width = request.BoundingBox.Width,
                Height = request.BoundingBox.Height
            },
            Confidence = request.Confidence,
            ThumbnailUrl = request.ThumbnailUrl,
            OriginalImageUrl = request.OriginalImageUrl,
            Embedding = request.Embedding,
            Emotion = request.Emotion,
            EmotionConfidence = request.EmotionConfidence ?? 0.0,
            IsVectorDbSynced = false,
            VectorDbId = null
        };
        _context.MemberFaces.Add(entity);
        entity.AddDomainEvent(new MemberFaceCreatedEvent(entity));
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created MemberFace {MemberFaceId} for Member {MemberId}.", entity.Id, request.MemberId);
        return Result<Guid>.Success(entity.Id);
    }
}
