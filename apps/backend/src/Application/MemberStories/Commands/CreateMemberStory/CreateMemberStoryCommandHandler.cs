using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // NEW: For Result<T>
using backend.Application.Files.Commands.UploadFileFromUrl; // NEW
using backend.Application.Files.DTOs; // Moved DTOs
using backend.Application.Members.Specifications; // NEW
using backend.Domain.Entities;
using backend.Domain.Events.MemberStories;
using backend.Domain.ValueObjects;
using static backend.Domain.Events.MemberStories.MemberStoryCreatedWithFacesEvent;
namespace backend.Application.MemberStories.Commands.CreateMemberStory;

public class CreateMemberStoryCommandHandler : IRequestHandler<CreateMemberStoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;
    public CreateMemberStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }
    public async Task<Result<Guid>> Handle(CreateMemberStoryCommand request, CancellationToken cancellationToken)
    {

        var familyId = await _context.Members
                                     .WithSpecification(new FamilyIdByMemberIdSpecification(request.MemberId))
                                     .FirstOrDefaultAsync(cancellationToken);
        if (familyId == Guid.Empty)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        if (!_authorizationService.CanManageFamily(familyId))
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var member = await _context.Members
                                   .WithSpecification(new MemberByIdWithStoriesAndFacesSpecification(request.MemberId))
                                   .FirstOrDefaultAsync(cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        var memberStory = new MemberStory
        {
            MemberId = request.MemberId,
            Title = request.Title,
            Story = request.Story,
            Year = request.Year,
            TimeRangeDescription = request.TimeRangeDescription,
            LifeStage = request.LifeStage,
            Location = request.Location
        };

        MemberStoryImage? primaryImage = null;

        if (!string.IsNullOrEmpty(request.TemporaryOriginalImageUrl))
        {
            string originalImageUrl = string.Empty;

            if (!string.IsNullOrEmpty(request.TemporaryOriginalImageUrl) && request.TemporaryOriginalImageUrl.Contains("/temp/"))
            {
                var originalUploadResult = await UploadImageFromTempUrlAsync(
                    request.TemporaryOriginalImageUrl,
                    "original_image",
                    familyId,
                    cancellationToken);
                if (!originalUploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(originalUploadResult.Error ?? "Failed to upload original image from temporary URL.", originalUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
                }
                originalImageUrl = originalUploadResult.Value?.Url ?? string.Empty;
            }
            else if (!string.IsNullOrEmpty(request.TemporaryOriginalImageUrl))
            {
                originalImageUrl = request.TemporaryOriginalImageUrl;
            }




            if (!string.IsNullOrEmpty(originalImageUrl))
            {
                primaryImage = new MemberStoryImage
                {
                    ImageUrl = originalImageUrl,
                };
                memberStory.MemberStoryImages.Add(primaryImage);
            }
        }


        member.AddStory(memberStory);
        _context.MemberStories.Add(memberStory);

        if (request.DetectedFaces != null && request.DetectedFaces.Any())
        {
            var facesDataForCreation = request.DetectedFaces.Select(df => new FaceDataForCreation(
                df.Id,
                df.MemberId.GetValueOrDefault(),
                new BoundingBox { X = df.BoundingBox.X, Y = df.BoundingBox.Y, Width = df.BoundingBox.Width, Height = df.BoundingBox.Height },
                df.Confidence,
                df.Thumbnail,
                df.Embedding ?? new List<double>(),
                df.Emotion,
                df.EmotionConfidence
            )).ToList();
            memberStory.AddDomainEvent(new MemberStoryCreatedWithFacesEvent(memberStory, facesDataForCreation));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(memberStory.Id);
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
        return await _mediator.Send(command, cancellationToken);
    }
}
