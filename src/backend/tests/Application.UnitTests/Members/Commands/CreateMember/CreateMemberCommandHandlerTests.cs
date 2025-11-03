
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public CreateMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateMemberCommand
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            Gender = "Male"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var createdMember = await _context.Members.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdMember.Should().NotBeNull();
        createdMember!.FirstName.Should().Be(command.FirstName);
        createdMember.LastName.Should().Be(command.LastName);
        createdMember.FamilyId.Should().Be(command.FamilyId);
        createdMember.DomainEvents.Should().ContainSingle(e => e is Domain.Events.Members.MemberCreatedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        var command = new CreateMemberCommand { FamilyId = familyId, FirstName = "Unauthorized", LastName = "Member" };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var nonExistentFamilyId = Guid.NewGuid();
        var command = new CreateMemberCommand { FamilyId = nonExistentFamilyId, FirstName = "John", LastName = "Doe" };
        _authorizationServiceMock.Setup(x => x.CanManageFamily(nonExistentFamilyId)).Returns(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Family with ID {nonExistentFamilyId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateMemberCommand
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var createdMember = await _context.Members.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdMember.Should().NotBeNull();
        createdMember!.Code.Should().NotBeNullOrEmpty();
        createdMember.Code.Should().StartWith("MEM-");
    }

    [Fact]
    public async Task Handle_ShouldSetNewRoot_AndUnsetOldRoot()
    {
        // Arrange
        var handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object);
        var familyId = Guid.NewGuid();
        var oldRoot = new Member("Old", "Root", "OR", familyId) { IsRoot = true };
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        _context.Members.Add(oldRoot);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateMemberCommand
        {
            FirstName = "New",
            LastName = "Root",
            FamilyId = familyId,
            IsRoot = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var newRoot = await _context.Members.FindAsync(result.Value);
        var oldRootAfter = await _context.Members.FindAsync(oldRoot.Id);


        // Assert
        result.IsSuccess.Should().BeTrue();
        newRoot.Should().NotBeNull();
        newRoot!.IsRoot.Should().BeTrue();
        oldRootAfter.Should().NotBeNull();
        oldRootAfter!.IsRoot.Should().BeFalse();
    }
}
