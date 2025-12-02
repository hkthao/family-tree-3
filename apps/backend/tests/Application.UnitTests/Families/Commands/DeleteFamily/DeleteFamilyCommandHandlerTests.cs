using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Events.Families;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly DeleteFamilyCommandHandler _handler;

    public DeleteFamilyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
        _handler = new DeleteFamilyCommandHandler(_context, _authorizationServiceMock.Object, _currentUserMock.Object, _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteFamilyAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var existingFamily = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _dateTimeMock.Setup(x => x.Now).Returns(now);

        var command = new DeleteFamilyCommand(familyId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var deletedFamily = await _context.Families.FindAsync(familyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedFamily.Should().NotBeNull();
        deletedFamily!.IsDeleted.Should().BeTrue();
        deletedFamily.DeletedBy.Should().Be(userId.ToString());
        deletedFamily.DeletedDate.Should().Be(now);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyDeletedEvent) &&
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var command = new DeleteFamilyCommand(Guid.NewGuid());
        _authorizationServiceMock.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new DeleteFamilyCommand(familyId);

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
