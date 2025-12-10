
using backend.Application.AI.DTOs; // NEW
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // NEW
using backend.Application.Files.DTOs;
using backend.Application.Files.UploadFile; // NEW
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using FluentAssertions;
using MediatR; // NEW
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IStringLocalizer<CreateMemberCommandHandler>> _localizerMock;
    private readonly Mock<IMemberRelationshipService> _memberRelationshipServiceMock;
    private readonly Mock<IMediator> _mediatorMock; // NEW
    private readonly CreateMemberCommandHandler _handler;

    public CreateMemberCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _localizerMock = new Mock<IStringLocalizer<CreateMemberCommandHandler>>();
        _memberRelationshipServiceMock = new Mock<IMemberRelationshipService>();
        _mediatorMock = new Mock<IMediator>(); // NEW
        _handler = new CreateMemberCommandHandler(_context, _authorizationServiceMock.Object, _localizerMock.Object, _memberRelationshipServiceMock.Object, _mediatorMock.Object); // Pass mediator mock
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake member image data"));
        var expectedAvatarUrl = "http://uploaded.example.com/member_avatar.png";

        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = expectedAvatarUrl }));

        var command = new CreateMemberCommand
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            Gender = "Male",
            AvatarBase64 = avatarBase64 // Use AvatarBase64
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdMember = await _context.Members.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdMember.Should().NotBeNull();
        createdMember!.FirstName.Should().Be(command.FirstName);
        createdMember.LastName.Should().Be(command.LastName);
        createdMember.FamilyId.Should().Be(command.FamilyId);
        createdMember.AvatarUrl.Should().Be(expectedAvatarUrl); // Assert against uploaded URL
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Members.MemberCreatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);
        // Ensure mediator is not called if not authorized
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));


        var command = new CreateMemberCommand { FamilyId = familyId, FirstName = "Unauthorized", LastName = "Member" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(nonExistentFamilyId)).Returns(true);
        // Ensure mediator is not called if family not found
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));


        var command = new CreateMemberCommand { FamilyId = nonExistentFamilyId, FirstName = "John", LastName = "Doe" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Family with ID {nonExistentFamilyId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        // Ensure mediator is not called for this case
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));


        var command = new CreateMemberCommand
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdMember = await _context.Members.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdMember.Should().NotBeNull();
        createdMember!.Code.Should().NotBeNullOrEmpty();
        createdMember.Code.Should().StartWith("MEM-");
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }

    [Fact]
    public async Task Handle_ShouldSetNewRoot_AndUnsetOldRoot()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var oldRoot = new Member("Old", "Root", "OR", familyId);
        oldRoot.SetAsRoot();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        _context.Members.Add(oldRoot);
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);
        // Ensure mediator is not called for this case
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));


        var command = new CreateMemberCommand
        {
            FirstName = "New",
            LastName = "Root",
            FamilyId = familyId,
            IsRoot = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var newRoot = await _context.Members.FindAsync(result.Value);
        var oldRootAfter = await _context.Members.FindAsync(oldRoot.Id);


        // Assert
        result.IsSuccess.Should().BeTrue();
        newRoot.Should().NotBeNull();
        newRoot!.IsRoot.Should().BeTrue();
        oldRootAfter.Should().NotBeNull();
        oldRootAfter!.IsRoot.Should().BeFalse();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }
}
