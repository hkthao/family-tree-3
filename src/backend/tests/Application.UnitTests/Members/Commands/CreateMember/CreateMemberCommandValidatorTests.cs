using backend.Application.Members.Commands.CreateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator;

    public CreateMemberCommandValidatorTests()
    {
        _validator = new CreateMemberCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenLastNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi LastName là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand với LastName rỗng.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho LastName với thông báo lỗi cụ thể.
        var command = new CreateMemberCommand { LastName = string.Empty, FirstName = "Test", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last Name cannot be empty.");
        // 💡 Giải thích: LastName là trường bắt buộc và không được rỗng.
    }

    [Fact]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FirstName là chuỗi rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand với FirstName rỗng.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho FirstName với thông báo lỗi cụ thể.
        var command = new CreateMemberCommand { LastName = "Test", FirstName = string.Empty, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First Name cannot be empty.");
        // 💡 Giải thích: FirstName là trường bắt buộc và không được rỗng.
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand với FamilyId rỗng.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho FamilyId với thông báo lỗi cụ thể.
        var command = new CreateMemberCommand { LastName = "Test", FirstName = "Test", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("FamilyId cannot be empty.");
        // 💡 Giải thích: FamilyId là trường bắt buộc và không được rỗng.
    }

    [Fact]
    public void ShouldHaveError_WhenDateOfDeathIsBeforeDateOfBirth()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DateOfDeath trước DateOfBirth.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand với DateOfDeath trước DateOfBirth.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho DateOfDeath với thông báo lỗi cụ thể.
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(2000, 1, 1),
            DateOfDeath = new DateTime(1999, 1, 1)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfDeath)
              .WithErrorMessage("DateOfDeath cannot be before DateOfBirth.");
        // 💡 Giải thích: DateOfDeath phải lớn hơn hoặc bằng DateOfBirth.
    }

    [Fact]
    public void ShouldHaveError_WhenGenderIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Gender không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand với Gender không hợp lệ.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho Gender với thông báo lỗi cụ thể.
        var command = new CreateMemberCommand
        {
            LastName = "Test",
            FirstName = "Test",
            FamilyId = Guid.NewGuid(),
            Gender = "InvalidGender"
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Gender)
              .WithErrorMessage("Gender must be 'Male', 'Female', or 'Other'.");
        // 💡 Giải thích: Gender chỉ chấp nhận các giá trị 'Male', 'Female', hoặc 'Other'.
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi command hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMemberCommand hợp lệ.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem không có lỗi validation nào.
        var command = new CreateMemberCommand
        {
            LastName = "Valid",
            FirstName = "Member",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = new DateTime(2020, 1, 1),
            Gender = "Male",
            AvatarUrl = "http://example.com/avatar.jpg",
            Occupation = "Engineer",
            Biography = "A valid biography.",
            Nickname = "Nick",
            PlaceOfBirth = "City A",
            PlaceOfDeath = "City B"
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Tất cả các trường đều hợp lệ, nên không có lỗi nào được mong đợi.
    }
}
