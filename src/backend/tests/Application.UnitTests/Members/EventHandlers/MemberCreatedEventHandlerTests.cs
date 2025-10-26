using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Members.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Events.Members;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Members.EventHandlers;

public class MemberCreatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberCreatedEventHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IDomainEventNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IGlobalSearchService> _mockGlobalSearchService;
    private readonly Mock<IFamilyTreeService> _mockFamilyTreeService;
    private readonly Mock<IUser> _mockUser;
    private readonly Fixture _fixture;
    private readonly MemberCreatedEventHandler _handler;

    public MemberCreatedEventHandlerTests()
    {
        _mockLogger = new Mock<ILogger<MemberCreatedEventHandler>>();
        _mockMediator = new Mock<IMediator>();
        _mockNotificationPublisher = new Mock<IDomainEventNotificationPublisher>();
        _mockGlobalSearchService = new Mock<IGlobalSearchService>();
        _mockFamilyTreeService = new Mock<IFamilyTreeService>();
        _mockUser = new Mock<IUser>();
        _fixture = new Fixture();

        _handler = new MemberCreatedEventHandler(
            _mockLogger.Object,
            _mockMediator.Object,
            _mockNotificationPublisher.Object,
            _mockGlobalSearchService.Object,
            _mockFamilyTreeService.Object,
            _mockUser.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xử lý sự kiện MemberCreatedEvent một cách chính xác,
    /// bao gồm ghi log, ghi lại hoạt động người dùng, xuất bản thông báo và cập nhật dịch vụ tìm kiếm toàn cầu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Member và một MemberCreatedEvent. Thiết lập _mockUser.Id.
    ///               Thiết lập các mock service để không ném ngoại lệ.
    ///    - Act: Gọi phương thức Handle của handler với sự kiện đã tạo.
    ///    - Assert: Kiểm tra rằng các phương thức của logger, mediator, notificationPublisher và globalSearchService
    ///              đã được gọi đúng số lần với các tham số phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng tất cả các hành động phụ trợ
    /// cần thiết sau khi thành viên được tạo đều được thực hiện một cách chính xác và đầy đủ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPerformAllRequiredActions_WhenMemberCreatedEventIsHandled()
    {
        // Arrange
        var member = _fixture.Build<Member>()
            .Without(m => m.Family)
            .Without(m => m.Relationships)
            .Create();
        var notification = new MemberCreatedEvent(member);
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(backend.Application.Common.Models.Result<Guid>.Success(Guid.NewGuid()));
        _mockNotificationPublisher.Setup(p => p.PublishNotificationForEventAsync(It.IsAny<MemberCreatedEvent>(), It.IsAny<CancellationToken>()))
                                  .Returns(Task.CompletedTask);
        _mockGlobalSearchService.Setup(g => g.UpsertEntityAsync(
            It.IsAny<Member>(),
            It.IsAny<string>(),
            It.IsAny<Func<Member, string>>(),
            It.IsAny<Func<Member, Dictionary<string, string>>>(),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask);
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Family Tree Domain Event") && v.ToString()!.Contains(notification.GetType().Name)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Member {notification.Member.FullName} ({notification.Member.Id}) was successfully created.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMediator.Verify(m => m.Send(
            It.Is<RecordActivityCommand>(cmd =>
                cmd.UserProfileId == userId &&
                cmd.ActionType == UserActionType.CreateMember &&
                cmd.TargetType == TargetType.Member &&
                cmd.TargetId == notification.Member.Id.ToString() &&
                cmd.ActivitySummary == $"Created member '{notification.Member.FullName}'."),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockNotificationPublisher.Verify(p => p.PublishNotificationForEventAsync(notification, It.IsAny<CancellationToken>()), Times.Once);

        _mockGlobalSearchService.Verify(g => g.UpsertEntityAsync(
            notification.Member,
            "Member",
            It.IsAny<Func<Member, string>>(),
            It.IsAny<Func<Member, Dictionary<string, string>>>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(notification.Member.FamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
