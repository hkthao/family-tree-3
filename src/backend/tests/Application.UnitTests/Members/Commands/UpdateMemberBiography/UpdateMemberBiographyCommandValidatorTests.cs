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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường MemberId của UpdateMemberBiographyCommand là Guid.Empty.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberBiographyCommand với MemberId là Guid.Empty.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính MemberId với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: MemberId là trường bắt buộc
    /// để xác định thành viên cần cập nhật tiểu sử và không được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.Empty, BiographyContent = _fixture.Create<string>() };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MemberId)
            .WithErrorMessage("MemberId cannot be empty.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi trường MemberId của UpdateMemberBiographyCommand hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberBiographyCommand với MemberId hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có lỗi validation cho thuộc tính MemberId.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: MemberId hợp lệ không gây ra lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenMemberIdIsProvided()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = _fixture.Create<string>() };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.MemberId);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator báo lỗi khi trường BiographyContent của UpdateMemberBiographyCommand
    /// vượt quá độ dài tối đa cho phép (1500 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent dài hơn 1500 ký tự.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng có lỗi validation cho thuộc tính BiographyContent với thông báo lỗi cụ thể.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: BiographyContent có giới hạn độ dài tối đa
    /// để tránh việc lưu trữ quá nhiều dữ liệu và đảm bảo hiệu suất.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenBiographyContentExceedsMaxLength()
    {
        var longBiographyContent = _fixture.Create<string>();
        while (longBiographyContent.Length <= 1500)
        {
            longBiographyContent += _fixture.Create<string>();
        }
        longBiographyContent = longBiographyContent[..1501]; // Đảm bảo chính xác 1501 ký tự

        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = longBiographyContent };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BiographyContent)
            .WithErrorMessage("Biography content must not exceed 1500 characters.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi trường BiographyContent của UpdateMemberBiographyCommand
    /// nằm trong giới hạn độ dài cho phép.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent có độ dài hợp lệ.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có lỗi validation cho thuộc tính BiographyContent.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: BiographyContent hợp lệ không gây ra lỗi.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenBiographyContentIsWithinMaxLength()
    {
        var validBiographyContent = _fixture.Create<string>().PadRight(100, 'a')[..100]; // Ensure at least 100 characters
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = validBiographyContent };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BiographyContent);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh validator không báo lỗi khi trường BiographyContent của UpdateMemberBiographyCommand là chuỗi rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UpdateMemberBiographyCommand với BiographyContent là chuỗi rỗng.
    ///    - Act: Thực hiện validate command bằng validator.
    ///    - Assert: Kiểm tra rằng không có lỗi validation cho thuộc tính BiographyContent.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: BiographyContent được phép để trống.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenBiographyContentIsEmpty()
    {
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid(), BiographyContent = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.BiographyContent);
    }
}
