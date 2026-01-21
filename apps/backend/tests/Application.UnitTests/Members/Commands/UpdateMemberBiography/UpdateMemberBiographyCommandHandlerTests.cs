using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMemberBiography;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using FluentAssertions;
using Microsoft.EntityFrameworkCore; // NEW
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly UpdateMemberBiographyCommandHandler _handler;

    public UpdateMemberBiographyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new UpdateMemberBiographyCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBiographyAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = CreateMember("John", "Doe", "JD", familyId, false, memberId);
        member.Update("John", "Doe", member.Code, null, null, null, null, null, null, null, null, null, null, null, null, "Old biography.", 0, false);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var command = new UpdateMemberBiographyCommand
        {
            MemberId = memberId,
            BiographyContent = "New biography."
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMember = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.Biography.Should().Be(command.BiographyContent);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Members.MemberBiographyUpdatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var command = new UpdateMemberBiographyCommand { MemberId = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {command.MemberId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var member = CreateMember("John", "Doe", "JD", familyId, false, memberId);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false);

        var command = new UpdateMemberBiographyCommand { MemberId = memberId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
