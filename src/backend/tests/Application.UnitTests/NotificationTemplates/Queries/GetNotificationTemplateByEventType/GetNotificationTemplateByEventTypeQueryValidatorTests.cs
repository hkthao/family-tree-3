using AutoFixture;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

public class GetNotificationTemplateByEventTypeQueryValidatorTests
{
    private readonly Fixture _fixture;

    public GetNotificationTemplateByEventTypeQueryValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường EventType của GetNotificationTemplateByEventTypeQuery
    /// là một giá trị enum không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplateByEventTypeQuery với EventType là một giá trị không hợp lệ.
    ///               Khởi tạo GetNotificationTemplateByEventTypeQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Loại sự kiện không hợp lệ.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường EventType phải là một giá trị hợp lệ trong enum NotificationType.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_InvalidEventType_ShouldReturnValidationError()
    {
        // Arrange
        var invalidEventType = (NotificationType)999; // An integer value not present in the enum
        var query = new GetNotificationTemplateByEventTypeQuery
        {
            EventType = invalidEventType,
            Channel = NotificationChannel.Email
        };

        var validator = new GetNotificationTemplateByEventTypeQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventType" && e.ErrorMessage == "Loại sự kiện không hợp lệ.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Channel của GetNotificationTemplateByEventTypeQuery
    /// là một giá trị enum không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplateByEventTypeQuery với Channel là một giá trị không hợp lệ.
    ///               Khởi tạo GetNotificationTemplateByEventTypeQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Kênh thông báo không hợp lệ.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Channel phải là một giá trị hợp lệ trong enum NotificationChannel.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_InvalidChannel_ShouldReturnValidationError()
    {
        // Arrange
        var invalidChannel = (NotificationChannel)999; // An integer value not present in the enum
        var query = new GetNotificationTemplateByEventTypeQuery
        {
            EventType = NotificationType.FamilyCreated,
            Channel = invalidChannel
        };

        var validator = new GetNotificationTemplateByEventTypeQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Channel" && e.ErrorMessage == "Kênh thông báo không hợp lệ.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không trả về lỗi khi GetNotificationTemplateByEventTypeQuery hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplateByEventTypeQuery hợp lệ.
    ///               Khởi tạo GetNotificationTemplateByEventTypeQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate không có lỗi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một query hợp lệ không nên gây ra lỗi validation.
    /// </summary>
    [Fact]
    public async Task Validate_ValidQuery_ShouldNotReturnValidationError()
    {
        // Arrange
        var query = new GetNotificationTemplateByEventTypeQuery
        {
            EventType = NotificationType.FamilyCreated,
            Channel = NotificationChannel.Email
        };

        var validator = new GetNotificationTemplateByEventTypeQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
