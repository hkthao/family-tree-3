using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Events.Members;
using Microsoft.Extensions.Localization;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemberCommandHandler> localizer, IMemberRelationshipService memberRelationshipService, IMediator mediator) : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IStringLocalizer<UpdateMemberCommandHandler> _localizer = localizer;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<Guid>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var family = await _context.Families
            .WithSpecification(new FamilyByIdSpecification(request.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);
        if (family == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        var member = await _context.Members
            .Include(m => m.SourceRelationships)
            .Include(m => m.TargetRelationships)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
        }

        string? finalAvatarUrl = member.AvatarUrl; // Keep current avatar URL by default

        // --- Handle AvatarBase64 upload ---
        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = request.FamilyId, // Add FamilyId
                    File = imageData,
                    FileName = $"Member_Avatar_{Guid.NewGuid()}.png",
                    Folder = string.Format(UploadConstants.MemberAvatarFolder, member.FamilyId),
                    MediaType = Domain.Enums.MediaType.Image // Explicitly set MediaType if known
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                // CreateFamilyMediaCommand returns a Guid (the ID of the new FamilyMedia record), not an object with a Url.
                // We need to fetch the FamilyMedia object to get its FilePath (URL).
                var familyMedia = await _context.FamilyMedia.FindAsync(uploadResult.Value!.Id);
                if (familyMedia == null || string.IsNullOrEmpty(familyMedia.FilePath))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                finalAvatarUrl = familyMedia.FilePath; // Update finalAvatarUrl
            }
            catch (FormatException)
            {
                return Result<Guid>.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
            }
        }
        else if (request.AvatarBase64 != null && request.AvatarBase64.Length == 0)
        {
            finalAvatarUrl = null; // Clear avatar if empty base64 is provided
        }
        // --- End Handle AvatarBase64 upload ---

        member.Update(
            request.FirstName,
            request.LastName,
            member.Code,
            request.Nickname,
            request.Gender,
            request.DateOfBirth,
            request.DateOfDeath,
            request.PlaceOfBirth,
            request.PlaceOfDeath,
            request.Phone,
            request.Email,
            request.Address,
            request.Occupation,
            finalAvatarUrl, // Pass finalAvatarUrl to update method
            request.Biography,
            request.Order,
            request.IsDeceased
        );

        // Handle IsRoot property update
        if (request.IsRoot)
        {
            // If the updated member should be the root
            var currentRoot = _context.Members.FirstOrDefault(m => m.IsRoot && m.Id != member.Id);
            currentRoot?.UnsetAsRoot(); // Unset the old root if it exists
            member.SetAsRoot(); // Set the current member as the new root
        }
        else if (member.IsRoot) // If the member was previously a root but now shouldn't be
        {
            member.UnsetAsRoot();
        }

        // Synchronize Birth and Death events (now handled by MemberUpdatedEventHandler)

        member.AddDomainEvent(new MemberUpdatedEvent(member));

        await _context.SaveChangesAsync(cancellationToken);

        // Cập nhật các mối quan hệ bằng phương thức mới
        await _memberRelationshipService.UpdateMemberRelationshipsAsync(
            member.Id,
            request.FatherId,
            request.MotherId,
            request.HusbandId,
            request.WifeId,
            cancellationToken
        );

        return Result<Guid>.Success(member.Id);
    }
}
