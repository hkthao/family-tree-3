
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IDateTime> _dateTimeMock;

    public DeleteMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _dateTimeMock = new Mock<IDateTime>();
    }

    [Fact]
    public async Task Handle_ShouldDeleteMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        family.AddMember(member);
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new DeleteMemberCommand(memberId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Use a fresh context to verify the deletion
        await using var verificationContext = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _currentUserMock.Object, _dateTimeMock.Object);
        var deletedMember = await verificationContext.Members.FindAsync(memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedMember.Should().BeNull();
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Members.MemberDeletedEvent) &&
            events.Any(e => e is Domain.Events.Families.FamilyStatsUpdatedEvent) // FamilyStatsUpdatedEvent is also raised
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var command = new DeleteMemberCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var handler = new DeleteMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        var command = new DeleteMemberCommand(memberId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
