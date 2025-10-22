using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using System;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandValidatorTests
{
    private readonly UpdateRelationshipCommandValidator _validator;

    public UpdateRelationshipCommandValidatorTests()
    {
        _validator = new UpdateRelationshipCommandValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Id trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand với Id là Guid.Empty.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Id với thông báo phù hợp.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.Empty,
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Id cannot be empty.");
        // 💡 Giải thích: Id là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand với SourceMemberId là Guid.Empty.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho SourceMemberId với thông báo phù hợp.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.Empty,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SourceMemberId);
        result.Errors.Should().Contain(e => e.ErrorMessage == "SourceMemberId cannot be empty.");
        // 💡 Giải thích: SourceMemberId là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenTargetMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi TargetMemberId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand với TargetMemberId là Guid.Empty.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho TargetMemberId với thông báo phù hợp.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.Empty,
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
        result.Errors.Should().Contain(e => e.ErrorMessage == "TargetMemberId cannot be empty.");
        // 💡 Giải thích: TargetMemberId là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceAndTargetMemberIdsAreSame()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberId và TargetMemberId giống nhau.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand với SourceMemberId và TargetMemberId giống nhau.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho TargetMemberId với thông báo phù hợp.
        var memberId = Guid.NewGuid();
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = memberId,
            TargetMemberId = memberId,
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TargetMemberId);
        result.Errors.Should().Contain(e => e.ErrorMessage == "SourceMemberId and TargetMemberId cannot be the same.");
        // 💡 Giải thích: SourceMemberId và TargetMemberId không được giống nhau.
    }

    [Fact]
    public void ShouldHaveErrorWhenTypeIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Type không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand với Type là một giá trị không hợp lệ.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Type với thông báo phù hợp.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = (RelationshipType)999, // Invalid enum value
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid RelationshipType value.");
        // 💡 Giải thích: Type phải là một giá trị hợp lệ của enum RelationshipType.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi UpdateRelationshipCommand hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateRelationshipCommand hợp lệ.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra không có lỗi validation.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: Command hợp lệ phải vượt qua validation.
    }
}
