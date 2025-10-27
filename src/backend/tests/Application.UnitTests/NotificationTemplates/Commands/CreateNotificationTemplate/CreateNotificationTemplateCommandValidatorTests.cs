using AutoFixture;
using backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.CreateNotificationTemplate;

public class CreateNotificationTemplateCommandValidatorTests
{
    private readonly Fixture _fixture;

    public CreateNotificationTemplateCommandValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Subject của CreateNotificationTemplateCommand bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với Subject là chuỗi rỗng.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Chủ đề không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Subject là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptySubject_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, string.Empty)
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Subject" && e.ErrorMessage == "Chủ đề không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Subject của CreateNotificationTemplateCommand
    /// vượt quá độ dài tối đa cho phép (250 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với Subject có độ dài lớn hơn 250 ký tự.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Chủ đề không được vượt quá 250 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Subject có giới hạn độ dài tối đa.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_SubjectExceedsMaxLength_ShouldReturnValidationError()
    {
        // Arrange
        var longSubject = new string('a', 251); // Subject with 251 characters
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, longSubject)
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Subject" && e.ErrorMessage == "Chủ đề không được vượt quá 250 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Body của CreateNotificationTemplateCommand bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với Body là chuỗi rỗng.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Nội dung không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Body là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptyBody_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, string.Empty)
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Body" && e.ErrorMessage == "Nội dung không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường LanguageCode của CreateNotificationTemplateCommand bị để trống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với LanguageCode là chuỗi rỗng.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Mã ngôn ngữ không được để trống.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường LanguageCode là bắt buộc và không được để trống.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_EmptyLanguageCode_ShouldReturnValidationError()
    {
        // Arrange
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, string.Empty)
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LanguageCode" && e.ErrorMessage == "Mã ngôn ngữ không được để trống.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường LanguageCode của CreateNotificationTemplateCommand
    /// vượt quá độ dài tối đa cho phép (10 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với LanguageCode có độ dài lớn hơn 10 ký tự.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Mã ngôn ngữ không được vượt quá 10 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường LanguageCode có giới hạn độ dài tối đa.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_LanguageCodeExceedsMaxLength_ShouldReturnValidationError()
    {
        // Arrange
        var longLanguageCode = new string('a', 11); // LanguageCode with 11 characters
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, longLanguageCode)
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LanguageCode" && e.ErrorMessage == "Mã ngôn ngữ không được vượt quá 10 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường EventType của CreateNotificationTemplateCommand
    /// là một giá trị enum không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với EventType là một giá trị không hợp lệ.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
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
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, invalidEventType)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventType" && e.ErrorMessage == "Loại sự kiện không hợp lệ.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Channel của CreateNotificationTemplateCommand
    /// là một giá trị enum không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với Channel là một giá trị không hợp lệ.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
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
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, invalidChannel)
            .With(c => c.Format, TemplateFormat.Html)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Channel" && e.ErrorMessage == "Kênh thông báo không hợp lệ.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi trường Format của CreateNotificationTemplateCommand
    /// là một giá trị enum không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateNotificationTemplateCommand với Format là một giá trị không hợp lệ.
    ///               Khởi tạo CreateNotificationTemplateCommandValidator.
    ///    - Act: Gọi phương thức Validate của validator với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Định dạng không hợp lệ.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Trường Format phải là một giá trị hợp lệ trong enum TemplateFormat.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_InvalidFormat_ShouldReturnValidationError()
    {
        // Arrange
        var invalidFormat = (TemplateFormat)999; // An integer value not present in the enum
        var command = _fixture.Build<CreateNotificationTemplateCommand>()
            .With(c => c.EventType, NotificationType.FamilyCreated)
            .With(c => c.Channel, NotificationChannel.Email)
            .With(c => c.Format, invalidFormat)
            .With(c => c.Subject, "Test Subject")
            .With(c => c.Body, "Test Body")
            .With(c => c.LanguageCode, "en")
            .With(c => c.IsActive, true)
            .Create();

        var validator = new CreateNotificationTemplateCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Format" && e.ErrorMessage == "Định dạng không hợp lệ.");
    }
}
