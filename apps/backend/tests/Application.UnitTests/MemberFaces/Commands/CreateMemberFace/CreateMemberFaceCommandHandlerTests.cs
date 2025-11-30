using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.SearchMemberFaces; // Changed to SearchMemberFaces
using backend.Application.MemberFaces.Queries.MemberFaces;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediatR; // Required for Unit.Value
using System.Linq;

namespace backend.Application.UnitTests.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<CreateMemberFaceCommandHandler>> _createLoggerMock;
    
    public CreateMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _createLoggerMock = new Mock<ILogger<CreateMemberFaceCommandHandler>>();
        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private CreateMemberFaceCommandHandler CreateCreateHandler()
    {
        return new CreateMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _createLoggerMock.Object);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldCreateNewMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var command = new CreateMemberFaceCommand
        {
            MemberId = member.Id,
            FaceId = "face123",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            ThumbnailUrl = "http://thumbnail.url",
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
        createdMemberFace.FaceId.Should().Be(command.FaceId);
        createdMemberFace.Confidence.Should().Be(command.Confidence);
        createdMemberFace.ThumbnailUrl.Should().Be(command.ThumbnailUrl);
    }

    [Fact]
    public async Task CreateMemberFace_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var command = new CreateMemberFaceCommand
        {
            MemberId = Guid.NewGuid(), // Non-existent member
            FaceId = "face123",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
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

        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var command = new CreateMemberFaceCommand
        {
            MemberId = member.Id,
            FaceId = "face123",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
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
