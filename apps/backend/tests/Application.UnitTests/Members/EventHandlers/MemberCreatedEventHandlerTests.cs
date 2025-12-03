
using backend.Application.Common.Interfaces;
using backend.Application.Members.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.EventHandlers;

public class MemberCreatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberCreatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IFamilyTreeService> _familyTreeServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly Mock<IStringLocalizer<MemberCreatedEventHandler>> _localizerMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly MemberCreatedEventHandler _handler;

    public MemberCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _familyTreeServiceMock = new Mock<IFamilyTreeService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
        _localizerMock = new Mock<IStringLocalizer<MemberCreatedEventHandler>>();
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new MemberCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _familyTreeServiceMock.Object, _currentUserMock.Object, _n8nServiceMock.Object, _localizerMock.Object, _contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity()
    {
        // Arrange
        var member = new Member("John", "Doe", "JD", Guid.NewGuid());
        var notification = new MemberCreatedEvent(member);
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));

        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family" };
        _contextMock.Setup(c => c.Families.FindAsync(It.IsAny<object[]>(), CancellationToken.None))
            .ReturnsAsync(family);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd =>
            cmd.ActionType == UserActionType.CreateMember &&
            cmd.ActivitySummary == _localizerMock.Object["Created member '{0}' in family '{1}'", member.FullName, family.Name].Value
        ), CancellationToken.None), Times.Once);
    }
}
