using backend.Application.Common.Interfaces;
using backend.Application.Members.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Events.Members;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediatR;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Members.EventHandlers;

public class MemberUpdatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberUpdatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IDomainEventNotificationPublisher> _notificationPublisherMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;

    public MemberUpdatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberUpdatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _notificationPublisherMock = new Mock<IDomainEventNotificationPublisher>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _currentUserMock = new Mock<ICurrentUser>();
    }

    /// <summary>
    /// Kiểm tra xem handler có ghi lại hoạt động, xuất bản thông báo và cập nhật tìm kiếm toàn cầu khi sự kiện được xử lý.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldRecordActivityPublishNotificationAndUpsertGlobalSearch_WhenEventIsHandled()
    {
        // Arrange
        var member = new Member("LastName", "FirstName", "CODE001", Guid.NewGuid()) { Id = Guid.NewGuid(), Biography = "Updated biography" };
        var notification = new MemberUpdatedEvent(member);
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.ProfileId).Returns(userId);

        var handler = new MemberUpdatedEventHandler(
            _loggerMock.Object,
            _mediatorMock.Object,
            _notificationPublisherMock.Object,
            _globalSearchServiceMock.Object,
            _currentUserMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));

        _mediatorMock.Verify(
            x => x.Send(It.Is<RecordActivityCommand>(cmd =>
                cmd.UserId == userId &&
                cmd.ActionType == UserActionType.UpdateMember &&
                cmd.TargetType == TargetType.Member &&
                cmd.TargetId == member.Id.ToString() &&
                cmd.ActivitySummary == $"Updated member '{member.FullName}' in family '{member.FamilyId}'."), It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationPublisherMock.Verify(
            x => x.PublishNotificationForEventAsync(notification, It.IsAny<CancellationToken>()),
            Times.Once);

        _globalSearchServiceMock.Verify(
            x => x.UpsertEntityAsync(
                member,
                "Member",
                It.IsAny<Func<Member, string>>(),
                It.IsAny<Func<Member, Dictionary<string, string>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
