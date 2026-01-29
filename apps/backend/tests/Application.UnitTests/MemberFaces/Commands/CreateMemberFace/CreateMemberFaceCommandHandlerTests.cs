using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Messages;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<CreateMemberFaceCommandHandler>> _createLoggerMock;

    private readonly Mock<IMessageBus> _messageBusMock;

    public CreateMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _createLoggerMock = new Mock<ILogger<CreateMemberFaceCommandHandler>>();
        _messageBusMock = new Mock<IMessageBus>();

        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private CreateMemberFaceCommandHandler CreateCreateHandler()
    {
        return new CreateMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _createLoggerMock.Object,_messageBusMock.Object);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldCreateNewMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var command = new CreateMemberFaceCommand
        {
            MemberId = member.Id,
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Thumbnail = "base64thumbnailstring", // Provide a base64 string
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = false
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
        createdMemberFace.ThumbnailUrl.Should().BeNull();
        createdMemberFace.IsVectorDbSynced.Should().BeTrue();
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
            MemberId = Guid.NewGuid(), // Non-existent member
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
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Unauthorized

        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var command = new CreateMemberFaceCommand
        {
            MemberId = member.Id,
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

    [Fact]
    public async Task CreateMemberFace_ShouldReturnFailure_WhenConflictDetected()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var conflictingMemberId = Guid.NewGuid();
        var conflictingFamily = new Family { Id = Guid.NewGuid(), Name = "Conflicting Family", Code = "CF" };
        var conflictingMember = new Member(conflictingMemberId, "Jane", "Doe", "JND", conflictingFamily.Id, conflictingFamily);

        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.Families.AddAsync(conflictingFamily);
        await _context.Members.AddAsync(conflictingMember);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(family.Id)).Returns(true);

        var command = new CreateMemberFaceCommand
        {
            MemberId = member.Id,
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Thumbnail = "base64thumbnailstring",
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = false
        };

        var handler = CreateCreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("similar embedding is already associated with another member");
        result.ErrorSource.Should().Be(ErrorSources.Conflict);

        // Ensure no MemberFace was added
        _context.MemberFaces.Should().BeEmpty();
    }
}
