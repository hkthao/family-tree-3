using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.GenerateFamilyKb;
using backend.Application.Files.Commands.UploadFileFromUrl; // Added
using backend.Application.Files.DTOs; // Added
using backend.Domain.Entities; // Added
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated

public class UpdateMemberStoryCommandHandler : IRequestHandler<UpdateMemberStoryCommand, Result> // Updated
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<UpdateMemberStoryCommandHandler> _localizer; // Updated
    private readonly IMediator _mediator; // Added
    private readonly ILogger<UpdateMemberStoryCommandHandler> _logger; // Added ILogger

    public UpdateMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemberStoryCommandHandler> localizer, IMediator mediator, ILogger<UpdateMemberStoryCommandHandler> logger) // Updated constructor
    {
        _context = context;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _mediator = mediator; // Added
        _logger = logger; // Initialized ILogger
    }

    public async Task<Result> Handle(UpdateMemberStoryCommand request, CancellationToken cancellationToken) // Updated
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var memberStory = await _context.MemberStories
            .Include(ms => ms.Member)
            .Include(ms => ms.MemberStoryImages) // Include images for update logic
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken); // Updated

        if (memberStory == null)
        {
            return Result.Failure(string.Format(ErrorMessages.NotFound, $"MemberStory with ID {request.Id}"), ErrorSources.NotFound); // Updated
        }

        memberStory.Update(
            request.Title,
            request.Story,
            request.Year,
            request.TimeRangeDescription,
            request.LifeStage,
            request.Location
        );

        // Remove existing images that are no longer in the request
        var incomingImageUrls = request.MemberStoryImageUrls ?? new List<string>();
        var existingImages = await _context.MemberStoryImages.Where(x => x.MemberStoryId == memberStory.Id).ToListAsync(cancellationToken);
        _context.MemberStoryImages.RemoveRange(existingImages.Where(ei => !incomingImageUrls.Contains(ei.ImageUrl)));

        // Add new images
        foreach (var imageUrl in incomingImageUrls)
        {
            if (!existingImages.Any(ei => ei.ImageUrl == imageUrl)) // Only add if it's a new image
            {
                // New image, process and add
                string processedImageUrl = imageUrl;

                // Handle temporary image URL (used for both original and resized)
                if (!string.IsNullOrEmpty(processedImageUrl) && processedImageUrl.Contains("/temp/"))
                {
                    var uploadResult = await UploadImageFromTempUrlAsync(
                        processedImageUrl,
                        "story_image", // Use a generic prefix
                        memberStory.Member.FamilyId,
                        cancellationToken);
                    if (!uploadResult.IsSuccess)
                    {
                        _logger.LogError("Failed to upload image from temporary URL: {Error}", uploadResult.Error);
                        processedImageUrl = string.Empty; // Clear URL if upload failed
                    }
                    else
                    {
                        processedImageUrl = uploadResult.Value?.Url ?? string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(processedImageUrl))
                {
                    memberStory.MemberStoryImages.Add(new MemberStoryImage
                    {
                        ImageUrl = processedImageUrl
                    });
                }
            }
        }
        await _context.SaveChangesAsync(cancellationToken);

        // Publish notification for story update
        await _mediator.Send(new GenerateFamilyKbCommand(memberStory.Member.FamilyId.ToString(), memberStory.Id.ToString(), KbRecordType.Story), cancellationToken);

        return Result.Success();
    }

    private async Task<Result<ImageUploadResponseDto>> UploadImageFromTempUrlAsync(
        string imageUrl, string fileNamePrefix, Guid familyId, CancellationToken cancellationToken)
    {
        var command = new UploadFileFromUrlCommand
        {
            FileUrl = imageUrl,
            FileName = $"{fileNamePrefix}_{Guid.NewGuid()}{Path.GetExtension(imageUrl)}",
            Folder = string.Format(UploadConstants.FamilyStoryPhotoFolder, familyId)
        };
        var uploadResult = await _mediator.Send(command, cancellationToken);
        if (uploadResult.IsSuccess && uploadResult.Value == null)
        {
            return Result<ImageUploadResponseDto>.Failure("Image upload succeeded but returned a null response.", ErrorSources.ExternalServiceError);
        }
        return uploadResult;
    }
}
