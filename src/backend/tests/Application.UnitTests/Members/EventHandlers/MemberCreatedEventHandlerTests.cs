
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.EventHandlers;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.EventHandlers;

public class MemberCreatedEventHandlerTests
{
    private readonly Mock<ILogger<MemberCreatedEventHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IGlobalSearchService> _globalSearchServiceMock;
    private readonly Mock<IFamilyTreeService> _familyTreeServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly MemberCreatedEventHandler _handler;

    public MemberCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<MemberCreatedEventHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _globalSearchServiceMock = new Mock<IGlobalSearchService>();
        _familyTreeServiceMock = new Mock<IFamilyTreeService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new MemberCreatedEventHandler(_loggerMock.Object, _mediatorMock.Object, _globalSearchServiceMock.Object, _familyTreeServiceMock.Object, _currentUserMock.Object, _n8nServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallServicesAndRecordActivity()
    {
        // Arrange
        var member = new Member("John", "Doe", "JD", Guid.NewGuid());
        var notification = new MemberCreatedEvent(member);
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<RecordActivityCommand>(cmd => cmd.ActionType == UserActionType.CreateMember), CancellationToken.None), Times.Once);
        _globalSearchServiceMock.Verify(s => s.UpsertEntityAsync(member, "Member", It.IsAny<Func<Member, string>>(), It.IsAny<Func<Member, Dictionary<string, string>>>(), CancellationToken.None), Times.Once);
        _n8nServiceMock.Verify(n => n.CallEmbeddingWebhookAsync(It.IsAny<EmbeddingWebhookDto>(), CancellationToken.None), Times.Once);
    }
}
