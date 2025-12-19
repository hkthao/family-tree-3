using backend.Application.Common.Constants;
using backend.Application.Common.Utils; // NEW

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
{
    private const int MAX_IMAGE_SIZE_MB = 5; // 5 MB
    private const int MAX_IMAGE_SIZE_BYTES = MAX_IMAGE_SIZE_MB * 1024 * 1024;

    public UpdateFamilyCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");

        RuleFor(v => v.Name)
            .NotNull().WithMessage("Name cannot be null.")
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(v => v.AvatarBase64)
            .Must(BeAValidBase64StringOrEmpty).WithMessage("AvatarBase64 phải là một chuỗi Base64 hợp lệ hoặc rỗng.")
            .When(v => v.AvatarBase64 != null)
            .Must(BeWithinImageSizeLimit).WithMessage(string.Format(ErrorMessages.FileSizeExceedsLimit, MAX_IMAGE_SIZE_MB))
            .When(v => !string.IsNullOrEmpty(v.AvatarBase64));

        RuleFor(v => v.Visibility)
            .NotNull().WithMessage("Visibility is required.")
            .NotEmpty().WithMessage("Visibility is required.")
            .Must(BeAValidVisibility).WithMessage("Visibility must be 'Public' or 'Private'.");
    }
    private bool BeAValidBase64StringOrEmpty(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return true;
        }
        try
        {
            ImageUtils.ConvertBase64ToBytes(base64String); // Use ImageUtils
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private bool BeWithinImageSizeLimit(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return true;
        }
        try
        {
            var imageData = ImageUtils.ConvertBase64ToBytes(base64String); // Use ImageUtils
            return imageData.Length <= MAX_IMAGE_SIZE_BYTES;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private bool BeAValidVisibility(string visibility)
    {
        return visibility == "Public" || visibility == "Private";
    }
}
