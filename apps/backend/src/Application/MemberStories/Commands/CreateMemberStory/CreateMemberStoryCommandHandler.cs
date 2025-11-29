using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.ValueObjects; // NEW
using backend.Application.Faces.Common; // NEW
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public class CreateMemberStoryCommandHandler : IRequestHandler<CreateMemberStoryCommand, Result<Guid>> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<CreateMemberStoryCommandHandler> _localizer; // Updated

    public CreateMemberStoryCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, IStringLocalizer<CreateMemberStoryCommandHandler> localizer) // Updated
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(CreateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        var memberStory = _mapper.Map<MemberStory>(request); // Updated
        memberStory.Id = Guid.NewGuid(); // Assign a new ID

        _context.MemberStories.Add(memberStory); // Updated
        await _context.SaveChangesAsync(cancellationToken);

        // Process and save detected faces
        if (request.DetectedFaces != null && request.DetectedFaces.Any())
        {
            foreach (var detectedFaceDto in request.DetectedFaces)
            {
                var memberFace = new MemberFace
                {
                    Id = Guid.NewGuid(),
                    MemberId = request.MemberId, // Link to the member from the story
                    FaceId = detectedFaceDto.Id,
                    BoundingBox = new BoundingBox
                    {
                        X = detectedFaceDto.BoundingBox.X,
                        Y = detectedFaceDto.BoundingBox.Y,
                        Width = detectedFaceDto.BoundingBox.Width,
                        Height = detectedFaceDto.BoundingBox.Height
                    },
                    Confidence = detectedFaceDto.Confidence,
                    ThumbnailUrl = detectedFaceDto.ThumbnailUrl,
                    OriginalImageUrl = request.PhotoUrl ?? request.OriginalImageUrl, // Use the photo/original image URL from the story
                    Embedding = detectedFaceDto.Embedding ?? new List<double>(), // Ensure embedding is not null
                    Emotion = detectedFaceDto.Emotion,
                    EmotionConfidence = (double?)detectedFaceDto.EmotionConfidence ?? 0.0
                };
                _context.MemberFaces.Add(memberFace);
            }
            await _context.SaveChangesAsync(cancellationToken); // Save the detected faces
        }

        return Result<Guid>.Success(memberStory.Id); // Updated
    }
}
