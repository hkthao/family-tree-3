using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Events.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.EventHandlers;

public class EventCreatedEventHandlerTests : TestBase
{
    private readonly EventCreatedEventHandler _handler;
    private readonly Mock<ILogger<EventCreatedEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IDomainEventNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IGlobalSearchService> _mockGlobalSearchService;

    public EventCreatedEventHandlerTests()
    {
        _mockLogger = _fixture.Freeze<Mock<ILogger<EventCreatedEventHandler>>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _mockNotificationPublisher = _fixture.Freeze<Mock<IDomainEventNotificationPublisher>>();
        _mockGlobalSearchService = _fixture.Freeze<Mock<IGlobalSearchService>>();

        _handler = new EventCreatedEventHandler(
            _mockLogger.Object,
            _mockMediator.Object,
            _mockNotificationPublisher.Object,
            _mockGlobalSearchService.Object,
            _mockUser.Object);
    }

    // Test cases will be added here

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng RecordActivityCommand được gửi khi một sự kiện được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventCreatedEvent với một sự kiện giả lập. Thiết lập _mockUser.Id để trả về một GUID hợp lệ. Thiết lập _mockMediator để trả về Result<Guid>.Success khi RecordActivityCommand được gửi.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem _mockMediator.Send đã được gọi một lần với một RecordActivityCommand có các thuộc tính phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc ghi lại hoạt động là một phần quan trọng của hệ thống để theo dõi các thay đổi và đảm bảo tính toàn vẹn của dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldRecordActivity_WhenEventIsCreated()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userProfileId);

        var @event = _fixture.Create<Event>();
        var notification = new EventCreatedEvent(@event);

        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd =>
                cmd.UserProfileId == userProfileId &&
                cmd.ActionType == Domain.Enums.UserActionType.CreateEvent &&
                cmd.TargetType == Domain.Enums.TargetType.Event &&
                cmd.TargetId == @event.Id.ToString() &&
                cmd.ActivitySummary == $"Created event '{@event.Name}'."
            ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng PublishNotificationForEventAsync được gọi khi một sự kiện được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventCreatedEvent với một sự kiện giả lập. Thiết lập _mockUser.Id để trả về một GUID hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem _mockNotificationPublisher.PublishNotificationForEventAsync đã được gọi một lần với notification và cancellationToken phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc xuất bản thông báo là một phần quan trọng của hệ thống để thông báo cho các thành phần khác về việc tạo sự kiện.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPublishNotification_WhenEventIsCreated()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userProfileId);

        var @event = _fixture.Create<Event>();
        var notification = new EventCreatedEvent(@event);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockNotificationPublisher.Verify(p => p.PublishNotificationForEventAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng UpsertEntityAsync được gọi trên IGlobalSearchService khi một sự kiện được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventCreatedEvent với một sự kiện giả lập. Thiết lập _mockUser.Id để trả về một GUID hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem _mockGlobalSearchService.UpsertEntityAsync đã được gọi một lần với các tham số phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc cập nhật dữ liệu sự kiện vào dịch vụ tìm kiếm toàn cầu là cần thiết để đảm bảo sự kiện có thể được tìm kiếm.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpsertEntityInGlobalSearchService_WhenEventIsCreated()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userProfileId);

        var @event = _fixture.Create<Event>();
        var notification = new EventCreatedEvent(@event);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockGlobalSearchService.Verify(g => g.UpsertEntityAsync(
                @event,
                "Event",
                It.IsAny<Func<Event, string>>(),
                It.IsAny<Func<Event, Dictionary<string, string>>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng LogInformation được gọi trên ILogger khi một sự kiện được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventCreatedEvent với một sự kiện giả lập. Thiết lập _mockUser.Id để trả về một GUID hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem _mockLogger.LogInformation đã được gọi ít nhất hai lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc ghi log là quan trọng để theo dõi luồng thực thi và gỡ lỗi trong môi trường sản xuất.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLogInformation_WhenEventIsCreated()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userProfileId);

        var @event = _fixture.Create<Event>();
        var notification = new EventCreatedEvent(@event);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }
}
