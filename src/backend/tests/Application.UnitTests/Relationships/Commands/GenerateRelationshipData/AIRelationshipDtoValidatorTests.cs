using backend.Application.Relationships.Commands.GenerateRelationshipData;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.GenerateRelationshipData;

public class AIRelationshipDtoValidatorTests
{
    private readonly AIRelationshipDtoValidator _validator;

    public AIRelationshipDtoValidatorTests()
    {
        _validator = new AIRelationshipDtoValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenSourceMemberNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi SourceMemberName trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một AIRelationshipDto với SourceMemberName rỗng.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho SourceMemberName với thông báo phù hợp.
        var dto = new AIRelationshipDto
        {
            SourceMemberName = string.Empty,
            TargetMemberName = "Target Name",
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SourceMemberName);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Source member name is required.");
        // 💡 Giải thích: SourceMemberName là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenTargetMemberNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi TargetMemberName trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một AIRelationshipDto với TargetMemberName rỗng.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho TargetMemberName với thông báo phù hợp.
        var dto = new AIRelationshipDto
        {
            SourceMemberName = "Source Name",
            TargetMemberName = string.Empty,
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.TargetMemberName);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Target member name is required.");
        // 💡 Giải thích: TargetMemberName là bắt buộc.
    }

    [Fact]
    public void ShouldHaveErrorWhenTypeIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi Type không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một AIRelationshipDto với Type là một giá trị không hợp lệ.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra có lỗi validation cho Type với thông báo phù hợp.
        var dto = new AIRelationshipDto
        {
            SourceMemberName = "Source Name",
            TargetMemberName = "Target Name",
            Type = (RelationshipType)999 // Invalid enum value
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid relationship type.");
        // 💡 Giải thích: Type phải là một giá trị hợp lệ của enum RelationshipType.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenDtoIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi AIRelationshipDto hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một AIRelationshipDto hợp lệ.
        // 2. Act: Gọi TestValidate trên validator.
        // 3. Assert: Kiểm tra không có lỗi validation.
        var dto = new AIRelationshipDto
        {
            SourceMemberName = "Source Name",
            TargetMemberName = "Target Name",
            Type = RelationshipType.Father
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
        // 💡 Giải thích: DTO hợp lệ phải vượt qua validation.
    }
}
