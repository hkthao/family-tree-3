using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IStringLocalizer<UpdateMemberCommandHandler>> _localizerMock;
    private readonly Mock<IMemberRelationshipService> _memberRelationshipServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _localizerMock = new Mock<IStringLocalizer<UpdateMemberCommandHandler>>();
        _memberRelationshipServiceMock = new Mock<IMemberRelationshipService>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new UpdateMemberCommandHandler(_context, _authorizationServiceMock.Object, _localizerMock.Object, _memberRelationshipServiceMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        family.AddMember(member);
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake member image data"));
        var expectedAvatarUrl = "http://uploaded.example.com/member_avatar.png";
        var familyMediaId = Guid.NewGuid();

        // Mock the mediator to return a successful Guid for CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(familyMediaId));

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
        var initialFather = new Member("Initial", "Father", "IF", familyId, false) { Id = initialFatherId };
        var newFather = new Member("New", "Father", "NF", familyId, false) { Id = newFatherId };

        family.AddMember(member);
        family.AddMember(initialFather);
        family.AddMember(newFather);
        member.AddFatherRelationship(initialFather.Id);

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // Update mediator mock to use CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Failure("Upload failed", "Test"));

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            FatherId = newFatherId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMember = await _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefaultAsync(m => m.Id == memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.TargetRelationships.Should().ContainSingle(r => r.SourceMemberId == newFatherId && r.TargetMemberId == memberId && r.Type == backend.Domain.Enums.RelationshipType.Father);
        updatedMember.TargetRelationships.Should().NotContain(r => r.SourceMemberId == initialFatherId);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
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
        var initialMother = new Member("Initial", "Mother", "IM", familyId, false) { Id = initialMotherId };
        var newMother = new Member("New", "Mother", "NM", familyId, false) { Id = newMotherId };

        family.AddMember(member);
        family.AddMember(initialMother);
        family.AddMember(newMother);
        member.AddMotherRelationship(initialMother.Id);

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // Update mediator mock to use CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Failure("Upload failed", "Test"));

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            MotherId = newMotherId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedMember = await _context.Members.Include(m => m.SourceRelationships).Include(m => m.TargetRelationships).FirstOrDefaultAsync(m => m.Id == memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.TargetRelationships.Should().ContainSingle(r => r.SourceMemberId == newMotherId && r.TargetMemberId == memberId && r.Type == backend.Domain.Enums.RelationshipType.Mother);
        updatedMember.TargetRelationships.Should().NotContain(r => r.SourceMemberId == initialMotherId);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSpouseRelationship_WhenSpouseIdIsChanged()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var initialSpouseId = Guid.NewGuid();
        var newSpouseId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId, false) { Id = memberId };
        member.Update("John", "Doe", "JD", null, "Male", null, null, null, null, null, null, null, null, null, null, null, false);
        var initialSpouse = new Member("Initial", "Spouse", "IS", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, false) { Id = initialSpouseId };
        var newSpouse = new Member("New", "Spouse", "NS", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, false) { Id = newSpouseId };

        family.AddMember(member);
        family.AddMember(initialSpouse);
        family.AddMember(newSpouse);
        var rel1 = member.AddWifeRelationship(initialSpouse.Id); // Changed
        _context.Relationships.Add(rel1); // Added rel1 to context


        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        // Update mediator mock to use CreateFamilyMediaCommand
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Failure("Upload failed", "Test"));

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            WifeId = newSpouseId // Changed to WifeId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Reload members after the command has completed and saved changes
        var updatedMember = await _context.Members
            .Include(m => m.SourceRelationships)
            .Include(m => m.TargetRelationships)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedMember.Should().NotBeNull();
        updatedMember!.SourceRelationships.Should().ContainSingle(r => r.SourceMemberId == memberId && r.TargetMemberId == newSpouseId && r.Type == backend.Domain.Enums.RelationshipType.Husband); // Changed assertion
        updatedMember.SourceRelationships.Should().NotContain(r => r.SourceMemberId == memberId && r.TargetMemberId == initialSpouseId); // Changed assertion
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
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
                     .ReturnsAsync(Result<Guid>.Failure("Upload failed", "Test"));

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
                     .ReturnsAsync(Result<Guid>.Failure("Upload failed", "Test"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFamilyMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
