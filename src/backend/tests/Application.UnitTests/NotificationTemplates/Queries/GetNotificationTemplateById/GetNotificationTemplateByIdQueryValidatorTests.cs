using AutoFixture;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplateById;

public class GetNotificationTemplateByIdQueryValidatorTests
{
    private readonly Fixture _fixture;

    public GetNotificationTemplateByIdQueryValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Id của GetNotificationTemplateByIdQuery bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplateByIdQuery với Id là Guid.Empty.
    ///               Khởi tạo GetNotificationTemplateByIdQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "ID không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Id là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetNotificationTemplateByIdQuery(Guid.Empty);
        var validator = new GetNotificationTemplateByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage == "ID không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không trả về lỗi khi GetNotificationTemplateByIdQuery hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplateByIdQuery hợp lệ.
    ///               Khởi tạo GetNotificationTemplateByIdQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate không có lỗi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một query hợp lệ không nên gây ra lỗi validation.
    /// </summary>
    [Fact]
    public async Task Validate_ValidQuery_ShouldNotReturnValidationError()
    {
        // Arrange
        var query = new GetNotificationTemplateByIdQuery(Guid.NewGuid());
        var validator = new GetNotificationTemplateByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
