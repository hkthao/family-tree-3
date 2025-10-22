using backend.Application.Relationships.Commands.CreateRelationship;
using FluentValidation.TestHelper;
using Xunit;
using System;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandValidatorTests
{
    private readonly CreateRelationshipCommandValidator _validator;

    public CreateRelationshipCommandValidatorTests()
    {
        _validator = new CreateRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId là Guid.Empty.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipCommand với SourceMemberId là Guid.Empty.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính SourceMemberId với thông báo phù hợp.
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.Empty,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.SourceMemberId)
            .WithErrorMessage("SourceMemberId cannot be empty.");
        // 💡 Giải thích: SourceMemberId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenTargetMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi TargetMemberId là Guid.Empty.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipCommand với TargetMemberId là Guid.Empty.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính TargetMemberId với thông báo phù hợp.
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.Empty,
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TargetMemberId)
            .WithErrorMessage("TargetMemberId cannot be empty.");
        // 💡 Giải thích: TargetMemberId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceMemberIdIsSameAsTargetMemberId()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId và TargetMemberId giống nhau.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipCommand với SourceMemberId và TargetMemberId giống nhau.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính TargetMemberId với thông báo phù hợp.
        var memberId = Guid.NewGuid();
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = memberId,
            TargetMemberId = memberId,
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TargetMemberId)
            .WithErrorMessage("SourceMemberId and TargetMemberId cannot be the same.");
        // 💡 Giải thích: Một thành viên không thể có mối quan hệ với chính mình.
    }

    [Fact]
    public void ShouldHaveErrorWhenTypeIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Type là giá trị không hợp lệ của enum RelationshipType.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipCommand với Type là giá trị không hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính Type với thông báo phù hợp.
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = (RelationshipType)999 // Invalid enum value
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Type)
            .WithErrorMessage("Invalid RelationshipType value.");
        // 💡 Giải thích: Type phải là một giá trị hợp lệ của enum RelationshipType.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi tất cả các trường đều hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một CreateRelationshipCommand với tất cả các trường hợp lệ.
        // 2. Act: Gọi phương thức TestValidate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi nào được báo cáo.
        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Một lệnh hợp lệ không nên gây ra bất kỳ lỗi xác thực nào.
    }
}