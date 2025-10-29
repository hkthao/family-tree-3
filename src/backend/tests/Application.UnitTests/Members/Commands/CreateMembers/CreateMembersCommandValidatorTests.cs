using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Queries;
using FluentValidation.TestHelper;
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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi danh sách thành viên trong CreateMembersCommand là rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand với danh sách Members rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Members với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Danh sách thành viên không được phép rỗng
    /// khi tạo nhiều thành viên cùng lúc.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenMembersListIsEmpty()
    {
        var command = new CreateMembersCommand([]);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Members)
              .WithErrorMessage("At least one member is required.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi danh sách thành viên
    /// trong CreateMembersCommand không rỗng và tất cả các thành viên đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand với danh sách Members không rỗng và chứa các AIMemberDto hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi danh sách thành viên không rỗng
    /// và mỗi thành viên đều hợp lệ, command phải được coi là hợp lệ và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenMembersListIsNotEmptyAndValid()
    {
        var validMembers = _fixture.CreateMany<AIMemberDto>(2).ToList();

        var command = new CreateMembersCommand(validMembers);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi có ít nhất một thành viên
    /// trong danh sách của CreateMembersCommand không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateMembersCommand với danh sách Members chứa một AIMemberDto không hợp lệ
    ///               (ví dụ: FirstName rỗng).
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính của phần tử không hợp lệ
    ///              trong danh sách (ví dụ: "Members[0].FirstName") với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu có bất kỳ thành viên nào trong danh sách
    /// không vượt qua validation, toàn bộ command CreateMembersCommand phải được coi là không hợp lệ
    /// và báo lỗi tương ứng.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenAMemberInListIsInvalid()
    {
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
    }
}
