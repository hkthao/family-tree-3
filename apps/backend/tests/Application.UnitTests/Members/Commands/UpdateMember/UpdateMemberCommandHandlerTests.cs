using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging; // Keep this one
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IMemberRelationshipService> _memberRelationshipServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<UpdateMemberCommandHandler>> _loggerMock;
    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _memberRelationshipServiceMock = new Mock<IMemberRelationshipService>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<UpdateMemberCommandHandler>>();
        _handler = new UpdateMemberCommandHandler(_context, _authorizationServiceMock.Object, _memberRelationshipServiceMock.Object, _mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member { Id = memberId, FirstName = "John", LastName = "Doe", Code = "JD", FamilyId = familyId, IsDeceased = false };
        family.AddMember(member);
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake member image data"));
        var expectedAvatarUrl = "http://uploaded.example.com/member_avatar.png";
        var familyMediaId = Guid.NewGuid();

        // Mock the mediator to return a successful FamilyMediaDto for CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Success(new FamilyMediaDto { Id = familyMediaId, FilePath = expectedAvatarUrl }));

        // Simulate the creation of FamilyMedia in the in-memory database
        // This is done because the handler calls _context.FamilyMedia.FindAsync
        _context.FamilyMedia.Add(new backend.Domain.Entities.FamilyMedia
        {
            Id = familyMediaId,
            FamilyId = familyId,
            FileName = "Member_Avatar_test.png",
            FilePath = expectedAvatarUrl,
            MediaType = Domain.Enums.MediaType.Image,
            FileSize = 12345,
            UploadedBy = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "Jane",
            LastName = "Smith",
            FamilyId = familyId,
            AvatarBase64 = avatarBase64 // Use AvatarBase64
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMember = await _context.Members.FindAsync(memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.AvatarUrl.Should().Be(expectedAvatarUrl); // Assert against uploaded URL
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Members.MemberUpdatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldUpdateFatherRelationship_WhenFatherIdIsChanged()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var initialFatherId = Guid.NewGuid();
        var newFatherId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        var initialFather = new Member { Id = initialFatherId, FirstName = "Initial", LastName = "Father", Code = "IF", FamilyId = familyId, IsDeceased = false };
        var newFather = new Member { Id = newFatherId, FirstName = "New", LastName = "Father", Code = "NF", FamilyId = familyId, IsDeceased = false };

        _context.Families.Add(family);
        _context.Members.AddRange(member, initialFather, newFather);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            FatherId = newFatherId,
            MotherId = null, // Ensure other relationships are not set
            HusbandId = null,
            WifeId = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _memberRelationshipServiceMock.Verify(s => s.UpdateMemberRelationshipsAsync(
            memberId,
            newFatherId,
            null,
            null,
            null,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMotherRelationship_WhenMotherIdIsChanged()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var initialMotherId = Guid.NewGuid();
        var newMotherId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        var initialMother = new Member { Id = initialMotherId, FirstName = "Initial", LastName = "Mother", Code = "IM", FamilyId = familyId, IsDeceased = false };
        var newMother = new Member { Id = newMotherId, FirstName = "New", LastName = "Mother", Code = "NM", FamilyId = familyId, IsDeceased = false };

        _context.Families.Add(family);
        _context.Members.AddRange(member, initialMother, newMother);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            FatherId = null, // Ensure other relationships are not set
            MotherId = newMotherId,
            HusbandId = null,
            WifeId = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _memberRelationshipServiceMock.Verify(s => s.UpdateMemberRelationshipsAsync(
            memberId,
            null,
            newMotherId,
            null,
            null,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSpouseRelationship_WhenSpouseIdIsChanged()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var newSpouseId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId);
        member.Id = memberId;
        member.UpdateGender("Male");
        var newSpouse = new Member("New", "Spouse", "NS", familyId);
        newSpouse.Id = newSpouseId;
        newSpouse.UpdateGender("Female");

        _context.Families.Add(family);
        _context.Members.AddRange(member, newSpouse);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            FatherId = null,
            MotherId = null,
            HusbandId = null,
            WifeId = newSpouseId // Updating the wife
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _memberRelationshipServiceMock.Verify(s => s.UpdateMemberRelationshipsAsync(
            memberId,
            null,
            null,
            null,
            newSpouseId,
            It.IsAny<CancellationToken>()
        ), Times.Once);
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

        // Update mediator mock to use CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = Guid.NewGuid(), FamilyId = familyId };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Update mediator mock to use CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<FamilyMediaDto>.Failure("Upload failed", "Test"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
