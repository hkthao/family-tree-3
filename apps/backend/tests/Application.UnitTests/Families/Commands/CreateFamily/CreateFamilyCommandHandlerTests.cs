using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // NEW
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Files.DTOs;
using backend.Application.Files.UploadFile; // NEW
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using FluentAssertions;
using MediatR; // NEW
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMediator> _mediatorMock; // NEW
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _mediatorMock = new Mock<IMediator>(); // NEW
        _handler = new CreateFamilyCommandHandler(_context, _currentUserMock.Object, _mediatorMock.Object); // Pass mediator mock
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var avatarBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake image data")); // Simulate image data
        var expectedAvatarUrl = "http://uploaded.example.com/avatar.png";

        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Success(new ImageUploadResponseDto { Url = expectedAvatarUrl }));

        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Description = "A family for testing",
            Address = "123 Test St",
            AvatarBase64 = avatarBase64, // Use AvatarBase64
            Visibility = "Public"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families
                                        .Include(f => f.FamilyUsers)
                                        .FirstOrDefaultAsync(f => f.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be(command.Name);
        createdFamily.Description.Should().Be(command.Description);
        createdFamily.Address.Should().Be(command.Address);
        createdFamily.AvatarUrl.Should().Be(expectedAvatarUrl); // Assert against expected uploaded URL
        createdFamily.Visibility.Should().Be(command.Visibility);
        createdFamily.Code.Should().StartWith("FAM-");
        createdFamily.FamilyUsers.Should().ContainSingle(fu => fu.UserId == userId && fu.Role == FamilyRole.Manager);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is FamilyCreatedEvent) &&
            events.Any(e => e is FamilyStatsUpdatedEvent)
        )), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Verify upload was called
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        // No AvatarBase64, so mediator should not be called
        _mediatorMock.Setup(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<ImageUploadResponseDto>.Failure("Upload failed", "Test"));

        var command = new CreateFamilyCommand
        {
            Name = "Family without code",
            Visibility = "Private"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamily = await _context.Families.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdFamily.Should().NotBeNull();
        createdFamily!.Code.Should().NotBeNullOrEmpty();
        createdFamily.Code.Should().StartWith("FAM-");
        _mediatorMock.Verify(m => m.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()), Times.Never); // Verify upload was NOT called
    }
}
