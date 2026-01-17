using backend.Application.Common.Constants; // NEW
using backend.Application.Common.Utils; // NEW

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    private const int MAX_IMAGE_SIZE_MB = 5; // 5 MB
    private const int MAX_IMAGE_SIZE_BYTES = MAX_IMAGE_SIZE_MB * 1024 * 1024;

    public CreateMemberCommandValidator()
    {
        RuleFor(v => v.LastName)
            .NotNull().WithMessage("Last Name cannot be null.")
            .NotEmpty().WithMessage("Last Name cannot be empty.")
            .MaximumLength(100).WithMessage("Last Name must not exceed 100 characters.");

        RuleFor(v => v.FirstName)
            .NotNull().WithMessage("First Name cannot be null.")
            .NotEmpty().WithMessage("First Name cannot be empty.")
            .MaximumLength(100).WithMessage("First Name must not exceed 100 characters.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId cannot be empty.");

        RuleFor(v => v.DateOfDeath)
            .GreaterThanOrEqualTo(v => v.DateOfBirth)
            .When(v => v.DateOfBirth.HasValue && v.DateOfDeath.HasValue)
            .WithMessage("DateOfDeath cannot be before DateOfBirth.");

        RuleFor(v => v.Gender)
            .Must(BeAValidGender).When(v => !string.IsNullOrEmpty(v.Gender))
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'.");

        RuleFor(v => v.AvatarUrl)
            .MaximumLength(2048).WithMessage("Avatar URL must not exceed 2048 characters.")
            .Matches(@"^(https?://)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*/?$").When(v => !string.IsNullOrEmpty(v.AvatarUrl))
            .WithMessage("Avatar URL must be a valid URL.");

        RuleFor(v => v.AvatarBase64)
            .Must(BeAValidBase64StringOrEmpty).WithMessage("AvatarBase64 phải là một chuỗi Base64 hợp lệ hoặc rỗng.")
            .When(v => v.AvatarBase64 != null)
            .Must(BeWithinImageSizeLimit).WithMessage(string.Format(ErrorMessages.FileSizeExceedsLimit, MAX_IMAGE_SIZE_MB))
            .When(v => !string.IsNullOrEmpty(v.AvatarBase64));

        RuleFor(v => v.Occupation)
            .MaximumLength(100).WithMessage("Occupation must not exceed 100 characters.");

        RuleFor(v => v.Biography)
            .MaximumLength(2000).WithMessage("Biography must not exceed 2000 characters.");

        RuleFor(v => v.Nickname)
            .MaximumLength(100).WithMessage("Nickname must not exceed 100 characters.");

        RuleFor(v => v.PlaceOfBirth)
            .MaximumLength(200).WithMessage("Place of Birth must not exceed 200 characters.");

        RuleFor(v => v.PlaceOfDeath)
            .MaximumLength(200).WithMessage("Place of Death must not exceed 200 characters.");

        RuleFor(v => v.Order)
            .GreaterThanOrEqualTo(0).When(v => v.Order.HasValue)
            .WithMessage("Order must be a positive number or zero.");
    }

    private bool BeAValidBase64StringOrEmpty(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return true;
        }
        try
        {
            ImageUtils.ConvertBase64ToBytes(base64String);
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
            var imageData = ImageUtils.ConvertBase64ToBytes(base64String);
            return imageData.Length <= MAX_IMAGE_SIZE_BYTES;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private bool BeAValidGender(string? gender)
    {
        if (string.IsNullOrEmpty(gender))
        {
            return true;
        }
        return gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
               gender.Equals("Female", StringComparison.OrdinalIgnoreCase) ||
               gender.Equals("Other", StringComparison.OrdinalIgnoreCase);
    }
}
