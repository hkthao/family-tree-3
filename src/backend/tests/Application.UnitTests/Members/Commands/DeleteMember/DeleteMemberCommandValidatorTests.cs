using backend.Application.Members.Commands.DeleteMember;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandValidatorTests
{
    private readonly DeleteMemberCommandValidator _validator;

    public DeleteMemberCommandValidatorTests()
    {
        _validator = new DeleteMemberCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Id là Guid rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một DeleteMemberCommand với Id rỗng.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem có lỗi validation cho Id với thông báo lỗi cụ thể.
        var command = new DeleteMemberCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id cannot be empty.");
        // 💡 Giải thích: Id là trường bắt buộc và không được rỗng.
    }

    [Fact]
    public void ShouldNotHaveError_WhenIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Id hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Tạo một DeleteMemberCommand với Id hợp lệ.
        // 2. Thực hiện validate command.
        // 3. Kiểm tra xem không có lỗi validation nào.
        var command = new DeleteMemberCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Id hợp lệ nên không có lỗi nào được mong đợi.
    }
}
