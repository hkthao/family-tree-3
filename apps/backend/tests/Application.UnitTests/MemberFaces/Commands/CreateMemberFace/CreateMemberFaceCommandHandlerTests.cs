using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Queries.SearchVectorFace;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Events.MemberFaces;
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
    private readonly Mock<IMediator> _mediatorMock; // NEW

    public CreateMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _createLoggerMock = new Mock<ILogger<CreateMemberFaceCommandHandler>>();
        _mediatorMock = new Mock<IMediator>(); // NEW

        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private CreateMemberFaceCommandHandler CreateCreateHandler()
    {
        return new CreateMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _createLoggerMock.Object, _mediatorMock.Object);
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
            FaceId = "face123",
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Thumbnail = "base64thumbnailstring", // Provide a base64 string
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = false
        };

        // Mock thumbnail upload service
        // Return a mocked URL

        // Mock mediator for SearchMemberFaceQuery
        _mediatorMock.Setup(m => m.Send(
            It.IsAny<SearchMemberFaceQuery>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>())); // No conflicts found

        var handler = CreateCreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Guid should be returned

        var createdMemberFace = await _context.MemberFaces.FindAsync(result.Value);
        createdMemberFace.Should().NotBeNull();
        createdMemberFace!.MemberId.Should().Be(command.MemberId);
        createdMemberFace.FaceId.Should().Be(command.FaceId);
        createdMemberFace.Confidence.Should().Be(command.Confidence);
        createdMemberFace.ThumbnailUrl.Should().BeNull(); // Assert against the mocked URL

        // Verify that the domain event was added
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is MemberFaceCreatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var command = new CreateMemberFaceCommand
        {
            MemberId = Guid.NewGuid(), // Non-existent member
            FaceId = "face123",
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Embedding = new List<double> { 0.1, 0.2, 0.3 }
        };

        // Mock thumbnail upload service (it won't be called if member not found, but good practice)


        // Mock mediator for SearchMemberFaceQuery (won't be called if member not found)
        _mediatorMock.Setup(m => m.Send(
            It.IsAny<SearchMemberFaceQuery>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>()));

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
            FaceId = "face123",
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Embedding = new List<double> { 0.1, 0.2, 0.3 }
        };

        // Mock thumbnail upload service (won't be called if unauthorized)


        // Mock mediator for SearchMemberFaceQuery (won't be called if unauthorized)
        _mediatorMock.Setup(m => m.Send(
            It.IsAny<SearchMemberFaceQuery>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>()));

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
            FaceId = "face123",
            BoundingBox = new BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            Thumbnail = "base64thumbnailstring",
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = false
        };

        // Mock thumbnail upload service


        // Mock mediator for SearchMemberFaceQuery to return a conflict
        _mediatorMock.Setup(m => m.Send(
            It.IsAny<SearchMemberFaceQuery>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<List<FoundFaceDto>>.Success(new List<FoundFaceDto>
        {
            new FoundFaceDto { MemberId = conflictingMemberId, FaceId = "conflictingFaceId", Score = 0.98f }
        }));

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
