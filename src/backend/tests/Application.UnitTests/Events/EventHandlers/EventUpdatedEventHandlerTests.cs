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

public class EventUpdatedEventHandlerTests : TestBase
{
    private readonly EventUpdatedEventHandler _handler;
    private readonly Mock<ILogger<EventUpdatedEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IDomainEventNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IGlobalSearchService> _mockGlobalSearchService;

    public EventUpdatedEventHandlerTests()
    {
        _mockLogger = _fixture.Freeze<Mock<ILogger<EventUpdatedEventHandler>>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _mockNotificationPublisher = _fixture.Freeze<Mock<IDomainEventNotificationPublisher>>();
        _mockGlobalSearchService = _fixture.Freeze<Mock<IGlobalSearchService>>();

        _handler = new EventUpdatedEventHandler(
            _mockLogger.Object,
            _mockMediator.Object,
            _mockNotificationPublisher.Object,
            _mockGlobalSearchService.Object,
            _mockUser.Object);
    }

    // Test cases will be added here

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng RecordActivityCommand được gửi khi một sự kiện được cập nhật.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một EventUpdatedEvent với một sự kiện giả lập. Thiết lập _mockUser.Id để trả về một GUID hợp lệ. Thiết lập _mockMediator để trả về Result<Guid>.Success khi RecordActivityCommand được gửi.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem _mockMediator.Send đã được gọi một lần với một RecordActivityCommand có các thuộc tính phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Việc ghi lại hoạt động là một phần quan trọng của hệ thống để theo dõi các thay đổi và đảm bảo tính toàn vẹn của dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldRecordActivity_WhenEventIsUpdated()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userProfileId);

        var @event = _fixture.Create<Event>();
        var notification = new EventUpdatedEvent(@event);

        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd =>
                cmd.UserProfileId == userProfileId &&
                cmd.ActionType == Domain.Enums.UserActionType.UpdateEvent &&
                cmd.TargetType == Domain.Enums.TargetType.Event &&
                cmd.TargetId == @event.Id.ToString() &&
                cmd.ActivitySummary == $"Updated event '{@event.Name}'."
            ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
