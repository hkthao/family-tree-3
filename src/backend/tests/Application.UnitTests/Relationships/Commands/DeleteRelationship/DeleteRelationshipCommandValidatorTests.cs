using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using System;
using backend.Application.Relationships.Commands.DeleteRelationship;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandValidatorTests
{
    private readonly DeleteRelationshipCommandValidator _validator;

    public DeleteRelationshipCommandValidatorTests()
    {
        _validator = new DeleteRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Id trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một DeleteRelationshipCommand với Id là Guid.Empty.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Id.
        var command = new DeleteRelationshipCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.Errors.Should().Contain(e => e.ErrorMessage == "ID mối quan hệ không được để trống.");
        // 💡 Giải thích: Id là bắt buộc.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi DeleteRelationshipCommand hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một DeleteRelationshipCommand hợp lệ.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra không có lỗi validation.
        var command = new DeleteRelationshipCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Command hợp lệ phải vượt qua validation.
    }
}
