using AutoFixture;
using backend.Application.Members.Commands.UpdateMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandValidatorTests
{
    private readonly UpdateMemberCommandValidator _validator;
    private readonly IFixture _fixture;

    public UpdateMemberCommandValidatorTests()
    {
        _validator = new UpdateMemberCommandValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Id trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với Id là Guid.Empty.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Id với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Id cannot be empty.");
        // 💡 Giải thích: Id là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenLastNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi LastName trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với LastName là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính LastName với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.LastName, string.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName)
            .WithErrorMessage("Last Name cannot be empty.");
        // 💡 Giải thích: LastName là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FirstName trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với FirstName là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FirstName với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.FirstName, string.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName)
            .WithErrorMessage("First Name cannot be empty.");
        // 💡 Giải thích: FirstName là trường bắt buộc và không được để trống.
    }
    [Fact]
    public void ShouldHaveErrorWhenFamilyIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi FamilyId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với FamilyId là Guid.Empty.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính FamilyId với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.FamilyId, Guid.Empty)
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FamilyId)
            .WithErrorMessage("FamilyId cannot be empty.");
        // 💡 Giải thích: FamilyId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenDateOfDeathIsBeforeDateOfBirth()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi DateOfDeath trước DateOfBirth.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với DateOfBirth và DateOfDeath không hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính DateOfDeath với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.DateOfBirth, new DateTime(2000, 1, 1))
            .With(c => c.DateOfDeath, new DateTime(1999, 1, 1))
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DateOfDeath)
            .WithErrorMessage("DateOfDeath cannot be before DateOfBirth.");
        // 💡 Giải thích: DateOfDeath không được phép trước DateOfBirth.
    }

    [Fact]
    public void ShouldHaveErrorWhenGenderIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Gender không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với Gender không hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Gender với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Gender, "InvalidGender")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Gender)
            .WithErrorMessage("Gender must be 'Male', 'Female', or 'Other'.");
        // 💡 Giải thích: Gender phải là một trong các giá trị hợp lệ.
    }

    [Fact]
    public void ShouldHaveErrorWhenAvatarUrlIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi AvatarUrl không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với AvatarUrl không hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính AvatarUrl với thông báo phù hợp.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.AvatarUrl, "invalid-url")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AvatarUrl)
            .WithErrorMessage("Avatar URL must be a valid URL.");
        // 💡 Giải thích: AvatarUrl phải là một URL hợp lệ.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenAllFieldsAreValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường đều hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberCommand với tất cả các trường hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi nào được báo cáo.
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.LastName, "ValidLastName")
            .With(c => c.FirstName, "ValidFirstName")
            .With(c => c.FamilyId, Guid.NewGuid())
            .With(c => c.DateOfBirth, new DateTime(1990, 1, 1))
            .With(c => c.DateOfDeath, new DateTime(2020, 1, 1))
            .With(c => c.Gender, "Male")
            .With(c => c.AvatarUrl, "https://valid.url/avatar.jpg")
            .With(c => c.Occupation, "Engineer")
            .With(c => c.Biography, "A short biography.")
            .With(c => c.Nickname, "Nick")
            .With(c => c.PlaceOfBirth, "City, Country")
            .With(c => c.PlaceOfDeath, "City, Country")
            .Create();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Khi tất cả các trường đều hợp lệ, validator không nên báo cáo lỗi nào.
    }
}
