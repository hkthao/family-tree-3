using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils; // New using
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.Users.Specifications;

namespace backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(IApplicationDbContext context, IMediator mediator, ICurrentUser currentUser) : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMediator _mediator = mediator; // New private field
    private readonly ICurrentUser _currentUser = currentUser; // New private field

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userSpec = new UserByProfileIdWithProfileSpec(request.Id);
        var user = await _context.Users
            .WithSpecification(userSpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || user.Profile == null)
        {
            return Result.Failure(ErrorMessages.UserProfileNotFound, ErrorSources.NotFound);
        }

        string avatarUrl = user.Profile.Avatar ?? string.Empty; // Initialize with existing avatar

        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    File = imageData,
                    FileName = $"User_Avatar_{Guid.NewGuid()}.png",
                    Folder = string.Format(UploadConstants.UserImagesFolder), // User specific folder
                    ContentType = "image/png", // Assuming PNG for now
                    FamilyId = null // This is a user avatar, not necessarily tied to a family.
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.FilePath))
                {
                    return Result.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                avatarUrl = uploadResult.Value.FilePath; // Update avatarUrl with the new FilePath
            }
            catch (FormatException)
            {
                return Result.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
            }
            catch (Exception ex)
            {
                return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
            }
        }
        else if (request.AvatarBase64 == "") // If AvatarBase64 is explicitly empty string, it means user wants to clear avatar
        {
            avatarUrl = string.Empty;
        }

        user.UpdateProfile(
            user.Profile.ExternalId, // ExternalId should not be updated here
            request.Email ?? user.Profile.Email,
            (request.FirstName != null || request.LastName != null) ? $"{request.FirstName ?? user.Profile.FirstName ?? string.Empty} {request.LastName ?? user.Profile.LastName ?? string.Empty}".Trim() : user.Profile.Name, // Combine first and last name if either is provided
            request.FirstName ?? user.Profile.FirstName ?? string.Empty,
            request.LastName ?? user.Profile.LastName ?? string.Empty,
            request.Phone ?? user.Profile.Phone ?? string.Empty,
            avatarUrl // Pass the determined avatar URL
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
