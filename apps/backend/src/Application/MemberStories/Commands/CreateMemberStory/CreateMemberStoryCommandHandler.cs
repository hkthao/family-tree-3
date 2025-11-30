using Ardalis.Specification.EntityFrameworkCore; // NEW
using backend.Application.AI.DTOs; // For ImageUploadResponseDto
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // NEW: For Result<T>
using backend.Application.MemberFaces.Commands.CreateMemberFace; // NEW: Using new CreateMemberFaceCommand
using backend.Application.Files.Commands.UploadFileFromUrl; // NEW
using backend.Application.Members.Specifications; // NEW
using backend.Domain.Entities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging; // NEW

namespace backend.Application.MemberStories.Commands.CreateMemberStory; // Updated

public class CreateMemberStoryCommandHandler : IRequestHandler<CreateMemberStoryCommand, Result<Guid>> // Updated
{
    private readonly IApplicationDbContext _context;

    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<CreateMemberStoryCommandHandler> _localizer; // Updated
    private readonly IMediator _mediator; // NEW
    private readonly ILogger<CreateMemberStoryCommandHandler> _logger; // NEW

    public CreateMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<CreateMemberStoryCommandHandler> localizer, IMediator mediator, ILogger<CreateMemberStoryCommandHandler> logger) // Updated
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _mediator = mediator; // NEW
        _logger = logger; // NEW
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
            OriginalImageUrl = (string?)request.OriginalImageUrl,
            ResizedImageUrl = request.ResizedImageUrl,
            RawInput = request.RawInput, // NEW
            StoryStyle = request.StoryStyle, // NEW
            Perspective = request.Perspective // NEW
        };

        // Check and upload OriginalImageUrl if it's a temporary URL
        if (!string.IsNullOrEmpty(request.OriginalImageUrl) && request.OriginalImageUrl.Contains("/temp/"))
        {
            var originalUploadResult = await UploadImageFromTempUrlAsync(
                request.OriginalImageUrl,
                "original_image", // Generic filename
                familyId,
                cancellationToken);

            if (!originalUploadResult.IsSuccess)
            {
                return Result<Guid>.Failure(originalUploadResult.Error ?? "Failed to upload original image from temporary URL.", originalUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
            }
            if (originalUploadResult.Value != null)
            {
                memberStory.OriginalImageUrl = originalUploadResult.Value.Url;
            }
        }

        // Check and upload ResizedImageUrl if it's a temporary URL
        if (!string.IsNullOrEmpty(request.ResizedImageUrl) && request.ResizedImageUrl.Contains("/temp/"))
        {
            var resizedUploadResult = await UploadImageFromTempUrlAsync(
                request.ResizedImageUrl,
                "resized_image", // Generic filename
                familyId,
                cancellationToken);

            if (!resizedUploadResult.IsSuccess)
            {
                return Result<Guid>.Failure(resizedUploadResult.Error ?? "Failed to upload resized image from temporary URL.", resizedUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
            }
            if (resizedUploadResult.Value != null)
            {
                memberStory.ResizedImageUrl = resizedUploadResult.Value.Url;
            }
        }
        // --- End logic to handle /temp/ URLs ---

        member.AddStory(memberStory); // Use the aggregate method
        _context.MemberStories.Add(memberStory); // Add to DbContext
        // Save all changes made to the aggregate
        await _context.SaveChangesAsync(cancellationToken);

        // Handle DetectedFaces if any
        foreach (var detectedFace in request.DetectedFaces)
        {
            var createFaceCommand = new CreateMemberFaceCommand
            {
                MemberId = request.MemberId,
                FaceId = detectedFace.Id,
                BoundingBox = detectedFace.BoundingBox,
                Confidence = (double)detectedFace.Confidence,
                OriginalImageUrl = request.OriginalImageUrl, // This is the original image URL for the story
                Embedding = detectedFace.Embedding ?? [],
                Thumbnail = detectedFace.Thumbnail, // Pass the base64 thumbnail
                Emotion = detectedFace.Emotion,
                EmotionConfidence = (double)(detectedFace.EmotionConfidence ?? 0.0f)
            };
            var createResult = await _mediator.Send(createFaceCommand, cancellationToken);
            if (!createResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create face {FaceId} for Member {MemberId}: {Error}", detectedFace.Id, request.MemberId, createResult.Error);
                // Continue processing other faces even if one fails
            }
        }

        return Result<Guid>.Success(memberStory.Id);
    }

    private async Task<Result<ImageUploadResponseDto>> UploadImageFromTempUrlAsync(
        string imageUrl, string fileNamePrefix, Guid familyId, CancellationToken cancellationToken)
    {
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = imageUrl,
            FileName = $"{fileNamePrefix}_{Guid.NewGuid()}{Path.GetExtension(imageUrl)}", // Generate unique name
            Folder = string.Format(UploadConstants.FamilyStoryPhotoFolder, familyId)
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
