
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new UpdateMemberCommandHandler(_context, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        family.AddMember(member.LastName, member.FirstName, member.Code);
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "Jane",
            LastName = "Smith",
            FamilyId = familyId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMember = await _context.Members.FindAsync(memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.DomainEvents.Should().ContainSingle(e => e is Domain.Events.Members.MemberUpdatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FamilyId = familyId };
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FamilyId = familyId };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
