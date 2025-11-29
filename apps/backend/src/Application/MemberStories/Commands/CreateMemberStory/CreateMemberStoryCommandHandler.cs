using Ardalis.Specification.EntityFrameworkCore; // NEW
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Specifications; // NEW
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Localization;

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public class CreateMemberStoryCommandHandler : IRequestHandler<CreateMemberStoryCommand, Result<Guid>> // Updated
{
    private readonly IApplicationDbContext _context;

    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<CreateMemberStoryCommandHandler> _localizer; // Updated

    public CreateMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<CreateMemberStoryCommandHandler> localizer) // Updated
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(CreateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Get FamilyId from Member
        var familyId = await _context.Members
                                     .WithSpecification(new FamilyIdByMemberIdSpecification(request.MemberId))
                                     .FirstOrDefaultAsync(cancellationToken);

        if (familyId == Guid.Empty) // Member not found or FamilyId is default
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        // Authorization check
        if (!_authorizationService.CanManageFamily(familyId)) // Check FamilyId for access
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Load the Member aggregate including its MemberStories and MemberFaces
        var member = await _context.Members
                                   .WithSpecification(new MemberByIdWithStoriesAndFacesSpecification(request.MemberId))
                                   .FirstOrDefaultAsync(cancellationToken);

        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        // Create MemberStory and add to the aggregate
        var memberStory = new MemberStory
        {
            MemberId = request.MemberId,
            Title = request.Title,
            Story = request.Story,
            PhotoUrl = request.PhotoUrl
        };
        member.AddStory(memberStory); // Use the aggregate method
        _context.MemberStories.Add(memberStory); // Add to DbContext
        // Process and save detected faces to the aggregate
        if (request.DetectedFaces != null && request.DetectedFaces.Count > 0)
        {
            foreach (var detectedFaceDto in request.DetectedFaces)
            {
                var memberFace = new MemberFace
                {
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
                member.AddFace(memberFace); // Use the aggregate method
                _context.MemberFaces.Add(memberFace); // Add to DbContext
            }
        }

        // Save all changes made to the aggregate
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(memberStory.Id);
    }
}
