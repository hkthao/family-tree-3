using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Messages;
using backend.Application.FamilyMedias.DTOs; // NEW
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Application.Common.Models; // NEW
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<CreateMemberFaceCommandHandler>> _createLoggerMock;

    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<IMediator> _mediatorMock;

    private Family? _family; // NEW
    private Member? _member; // NEW
    private string? _expectedThumbnailUrl; // NEW

    public CreateMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _createLoggerMock = new Mock<ILogger<CreateMemberFaceCommandHandler>>();
        _messageBusMock = new Mock<IMessageBus>();
        _mediatorMock = new Mock<IMediator>();

        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private CreateMemberFaceCommandHandler CreateCreateHandler()
    {
        return new CreateMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _createLoggerMock.Object, _messageBusMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldCreateNewMemberFace_WhenAuthorized()
    {
        // Arrange
        _family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _member = new Member(Guid.NewGuid(), "John", "Doe", "JD", _family!.Id, _family!);
        await _context.Families.AddAsync(_family);
        await _context.Members.AddAsync(_member);
        await _context.SaveChangesAsync();

        _expectedThumbnailUrl = "http://mocked.thumbnail.url/image.png";
        _mediatorMock.Setup(m => m.Send(
            It.IsAny<CreateFamilyMediaCommand>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<FamilyMediaDto>.Success(new FamilyMediaDto { FilePath = _expectedThumbnailUrl }));

        var command = new CreateMemberFaceCommand
        {
            MemberId = _member.Id,
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            Thumbnail = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=" // Example base64 PNG for a 1x1 transparent image
        };

        var handler = CreateCreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Guid should be returned

        var createdMemberFace = await _context.MemberFaces.FindAsync(result.Value);
        createdMemberFace.Should().NotBeNull();
        createdMemberFace!.MemberId.Should().Be(command.MemberId);
        createdMemberFace.Confidence.Should().Be(command.Confidence);
        createdMemberFace.ThumbnailUrl.Should().Be(_expectedThumbnailUrl); // Assert against the mocked URL
        createdMemberFace.VectorDbId.Should().NotBeNullOrEmpty();
        createdMemberFace.VectorDbId.Should().Be(createdMemberFace.Id.ToString());

        // Verify that the message was published to RabbitMQ
        _messageBusMock.Verify(m => m.PublishAsync(
            "face_exchange",
            "face.add",
            It.IsAny<MemberFaceAddedMessage>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var command = new CreateMemberFaceCommand
        {
            MemberId = System.Guid.NewGuid(), // Non-existent member
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Embedding = new List<double> { 0.1, 0.2, 0.3 }
        };

        var handler = CreateCreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldReturnFailure_WhenUnauthorized()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Deny authorization

        _family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _member = new Member(Guid.NewGuid(), "John", "Doe", "JD", _family!.Id, _family);
        await _context.Families.AddAsync(_family);
        await _context.Members.AddAsync(_member);
        await _context.SaveChangesAsync();

        var command = new CreateMemberFaceCommand
        {
            MemberId = _member.Id,
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Embedding = new List<double> { 0.1, 0.2, 0.3 }
        };

        var handler = CreateCreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

}
