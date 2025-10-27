using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.EventHandlers;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Relationships;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.EventHandlers;

public class RelationshipCreatedEventHandlerTests : TestBase
{

    private readonly Mock<ILogger<RelationshipCreatedEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IDomainEventNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IGlobalSearchService> _mockGlobalSearchService;
    private readonly RelationshipCreatedEventHandler _handler;

    public RelationshipCreatedEventHandlerTests() : base()
    {

        _mockLogger = new Mock<ILogger<RelationshipCreatedEventHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockNotificationPublisher = new Mock<IDomainEventNotificationPublisher>();
        _mockGlobalSearchService = new Mock<IGlobalSearchService>();

        _handler = new RelationshipCreatedEventHandler(

            _mockLogger.Object,
            _mockMediator.Object,
            _mockNotificationPublisher.Object,
            _mockGlobalSearchService.Object,
            _mockUser.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler thực hiện tất cả các hành động cần thiết
    /// khi một sự kiện RelationshipCreatedEvent được xử lý.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Relationship và một RelationshipCreatedEvent.
    ///               Thiết lập _mockUser để trả về một User ID hợp lệ.
    ///               Thiết lập các mock cho IMediator, IDomainEventNotificationPublisher và IGlobalSearchService.
    ///    - Act: Gọi phương thức Handle của handler với sự kiện đã tạo.
    ///    - Assert: Kiểm tra xem _mockLogger đã được gọi với LogInformation.
    ///              Kiểm tra xem _mockMediator.Send đã được gọi với RecordActivityCommand.
    ///              Kiểm tra xem _mockNotificationPublisher.PublishNotificationForEventAsync đã được gọi.
    ///              Kiểm tra xem _mockGlobalSearchService.UpsertEntityAsync đã được gọi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi một mối quan hệ được tạo,
    /// hệ thống phải ghi lại hoạt động, xuất bản thông báo và cập nhật dịch vụ tìm kiếm toàn cầu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPerformAllRequiredActions_WhenRelationshipCreatedEventIsHandled()
    {
        // Arrange
        var relationship = _fixture.Create<Relationship>();
        var notification = new RelationshipCreatedEvent(relationship);
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert


        _mockMediator.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd =>
                cmd.UserProfileId == userId &&
                cmd.ActionType == UserActionType.CreateRelationship &&
                cmd.TargetType == TargetType.Relationship &&
                cmd.TargetId == relationship.Id.ToString() &&
                cmd.ActivitySummary == $"Created relationship between {relationship.SourceMemberId} and {relationship.TargetMemberId} (Type: {relationship.Type})."), 
            It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationPublisher.Verify(p => p.PublishNotificationForEventAsync(notification, It.IsAny<CancellationToken>()), Times.Once);

        _mockGlobalSearchService.Verify(g => g.UpsertEntityAsync(
            relationship,
            "Relationship",
            It.IsAny<Func<Relationship, string>>(),
            It.IsAny<Func<Relationship, Dictionary<string, string>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
