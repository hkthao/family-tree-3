using backend.Application.Members.Commands.DeleteMember;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandValidatorTests
{
    private readonly DeleteMemberCommandValidator _validator;

    public DeleteMemberCommandValidatorTests()
    {
        _validator = new DeleteMemberCommandValidator();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường Id của DeleteMemberCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteMemberCommand với Id được đặt là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính Id với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Id là trường bắt buộc
    /// để xác định thành viên cần xóa và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new DeleteMemberCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi trường Id của DeleteMemberCommand hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteMemberCommand với Id được đặt là một Guid hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có bất kỳ lỗi validation nào được báo cáo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi Id hợp lệ, command phải được coi là hợp lệ
    /// và không có lỗi nào được trả về.
    /// </summary>
    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        var command = new DeleteMemberCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
