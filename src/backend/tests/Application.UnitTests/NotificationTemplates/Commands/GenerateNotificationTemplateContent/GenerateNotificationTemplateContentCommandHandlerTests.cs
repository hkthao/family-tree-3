using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.NotificationTemplates.Commands.GenerateNotificationTemplateContent;

public class GenerateNotificationTemplateContentCommandHandlerTests : TestBase
{
    private readonly GenerateNotificationTemplateContentCommandHandler _handler;
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;

    public GenerateNotificationTemplateContentCommandHandlerTests()
    {
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockChatProvider = _fixture.Freeze<Mock<IChatProvider>>();

        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateNotificationTemplateContentCommandHandler(
            _mockChatProviderFactory.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi AI trả về một phản hồi trống hoặc null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi trống.
    ///               Tạo một GenerateNotificationTemplateContentCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI không tạo ra phản hồi, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);
        var command = _fixture.Create<GenerateNotificationTemplateContentCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.NoAIResponse);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về nội dung được tạo
    /// khi AI trả về một chuỗi JSON hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ.
    ///               Tạo một GenerateNotificationTemplateContentCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa chủ đề và nội dung được phân tích chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể
    /// phân tích cú pháp JSON hợp lệ từ AI và trích xuất chủ đề và nội dung một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnGeneratedContent_WhenAIResponseIsValidJson()
    {
        // Arrange
        var validJson = "{ \"subject\": \"Test Subject\", \"body\": \"Test Body Content\" }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);
        var command = _fixture.Create<GenerateNotificationTemplateContentCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Subject.Should().Be("Test Subject");
        result.Value.Body.Should().Be("Test Body Content");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về nội dung được tạo
    /// khi AI trả về một chuỗi JSON không hợp lệ (toàn bộ nội dung được coi là body, subject trống).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
    ///               Tạo một GenerateNotificationTemplateContentCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa nội dung không hợp lệ làm body, chủ đề trống.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI tạo ra JSON không hợp lệ bằng cách coi toàn bộ phản hồi là nội dung body và để trống chủ đề.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnGeneratedContent_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        var invalidJson = "This is not a valid JSON string.";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(invalidJson);
        var command = _fixture.Create<GenerateNotificationTemplateContentCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Subject.Should().Be(string.Empty);
        result.Value.Body.Should().Be(invalidJson);
    }
}
