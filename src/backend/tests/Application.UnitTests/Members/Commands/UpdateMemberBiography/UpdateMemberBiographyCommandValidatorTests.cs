using AutoFixture;
using backend.Application.Members.Commands.UpdateMemberBiography;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandValidatorTests
{
    private readonly UpdateMemberBiographyCommandValidator _validator;
    private readonly IFixture _fixture;

    public UpdateMemberBiographyCommandValidatorTests()
    {
        _validator = new UpdateMemberBiographyCommandValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi MemberId trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberBiographyCommand với MemberId là Guid.Empty.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính MemberId với thông báo phù hợp.
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.Empty, BiographyContent = _fixture.Create<string>() };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MemberId)
            .WithErrorMessage("MemberId cannot be empty.");
        // 💡 Giải thích: MemberId là trường bắt buộc và không được để trống.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenMemberIdIsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi MemberId được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberBiographyCommand với MemberId hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính MemberId.
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = _fixture.Create<string>() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.MemberId);
        // 💡 Giải thích: MemberId hợp lệ không gây ra lỗi.
    }

    [Fact]
    public void ShouldHaveErrorWhenBiographyContentExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi BiographyContent vượt quá 1500 ký tự.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent dài hơn 1500 ký tự.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng có lỗi cho thuộc tính BiographyContent với thông báo phù hợp.
        var longBiographyContent = _fixture.Create<string>();
        while (longBiographyContent.Length <= 1500)
        {
            longBiographyContent += _fixture.Create<string>();
        }
        longBiographyContent = longBiographyContent.Substring(0, 1501); // Đảm bảo chính xác 1501 ký tự

        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = longBiographyContent };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BiographyContent)
            .WithErrorMessage("Biography content must not exceed 1500 characters.");
        // 💡 Giải thích: BiographyContent không được vượt quá 1500 ký tự.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenBiographyContentIsWithinMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi BiographyContent nằm trong giới hạn độ dài.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent có độ dài hợp lệ.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính BiographyContent.
        var validBiographyContent = _fixture.Create<string>().PadRight(100, 'a').Substring(0, 100); // Ensure at least 100 characters
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = validBiographyContent };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BiographyContent);
        // 💡 Giải thích: BiographyContent hợp lệ không gây ra lỗi.
    }

    [Fact]
    public void ShouldNotHaveErrorWhenBiographyContentIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi BiographyContent trống (vì nó không có .NotEmpty()).
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent là chuỗi rỗng.
        // 2. Act: Gọi phương thức Validate của validator.
        // 3. Assert: Kiểm tra rằng không có lỗi cho thuộc tính BiographyContent.
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BiographyContent);
        // 💡 Giải thích: BiographyContent được phép để trống.
    }
}
