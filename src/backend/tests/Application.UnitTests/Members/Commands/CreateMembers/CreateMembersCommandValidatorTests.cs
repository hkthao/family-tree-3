using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Queries;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMembers;

public class CreateMembersCommandValidatorTests
{
    private readonly CreateMembersCommandValidator _validator;
    private readonly IFixture _fixture;

    public CreateMembersCommandValidatorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Customize<AIMemberDto>(c => c.With(x => x.Gender, "Male")); // Ensure valid gender for AIMemberDto
        _validator = new CreateMembersCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenMembersListIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi danh sách thành viên rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMembersCommand với danh sách Members rỗng.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho Members với thông báo lỗi cụ thể.
        var command = new CreateMembersCommand(new List<AIMemberDto>());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Members)
              .WithErrorMessage("At least one member is required.");
        // 💡 Giải thích: Danh sách thành viên không được rỗng.
    }

    [Fact]
    public void ShouldNotHaveError_WhenMembersListIsNotEmptyAndValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi danh sách thành viên không rỗng và hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMembersCommand với danh sách Members không rỗng.
        // 2. Mock _mockAIMemberDtoValidator để trả về thành công cho mỗi thành viên.
        // 3. Thực hiện validate command.
        // 4. Kiểm tra xem không có lỗi validation nào.
        var validMembers = _fixture.CreateMany<AIMemberDto>(2).ToList();

        var command = new CreateMembersCommand(validMembers);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Khi danh sách thành viên không rỗng và mỗi thành viên đều hợp lệ, không nên có lỗi.
    }

    [Fact]
    public void ShouldHaveError_WhenAMemberInListIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi có một thành viên trong danh sách không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một CreateMembersCommand với danh sách Members chứa một thành viên không hợp lệ.
        // 2. Mock _mockAIMemberDtoValidator để trả về lỗi cho thành viên không hợp lệ.
        // 3. Thực hiện validate command.
        // 4. Kiểm tra xem có lỗi validation cho phần tử không hợp lệ trong danh sách.
        var invalidMember = new AIMemberDto // Tạo thủ công để đảm bảo FirstName rỗng
        {
            FirstName = string.Empty,
            LastName = _fixture.Create<string>(),
            Gender = "Male",
            DateOfBirth = _fixture.Create<DateTime>(),
            FamilyName = _fixture.Create<string>()
        };
        var members = new List<AIMemberDto> { invalidMember };

        var command = new CreateMembersCommand(members);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Members[0].FirstName")
              .WithErrorMessage("First name is required.");
        // 💡 Giải thích: Khi có ít nhất một thành viên không hợp lệ trong danh sách, validator phải báo lỗi.
    }
}
