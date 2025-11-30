using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Commands.UpdateMemberFace;

public class UpdateMemberFaceCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<UpdateMemberFaceCommandHandler>> _loggerMock;

    public UpdateMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<UpdateMemberFaceCommandHandler>>();
        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private UpdateMemberFaceCommandHandler CreateHandler()
    {
        return new UpdateMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _loggerMock.Object);
    }

    private async Task<MemberFace> SeedMemberFace(Family family, Member member)
    {
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        var memberFace = new MemberFace
        {
            MemberId = member.Id,
            FaceId = "initialFaceId",
            BoundingBox = new BoundingBox { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.9,
            ThumbnailUrl = "http://initial.thumbnail.url",
            OriginalImageUrl = "http://initial.original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "neutral",
            EmotionConfidence = 0.5,
            IsVectorDbSynced = false,
            VectorDbId = "initialVectorDbId"
        };
        await _context.MemberFaces.AddAsync(memberFace);
        await _context.SaveChangesAsync();
        return memberFace;
    }

    [Fact]
    public async Task UpdateMemberFace_ShouldUpdateMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var existingMemberFace = await SeedMemberFace(family, member);

        var command = new UpdateMemberFaceCommand
        {
            Id = existingMemberFace.Id,
            MemberId = member.Id,
            FaceId = "updatedFaceId",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 15, Y = 25, Width = 55, Height = 65 },
            Confidence = 0.95,
            ThumbnailUrl = "http://updated.thumbnail.url",
            OriginalImageUrl = "http://updated.original.url",
            Embedding = new List<double> { 0.4, 0.5, 0.6 },
            Emotion = "happy",
            EmotionConfidence = 0.99,
            IsVectorDbSynced = true,
            VectorDbId = "updatedVectorDbId"
        };

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedMemberFace = await _context.MemberFaces.AsNoTracking().FirstOrDefaultAsync(mf => mf.Id == existingMemberFace.Id);
        updatedMemberFace.Should().NotBeNull();
        updatedMemberFace!.FaceId.Should().Be(command.FaceId);
        updatedMemberFace.Confidence.Should().Be(command.Confidence);
        updatedMemberFace.ThumbnailUrl.Should().Be(command.ThumbnailUrl);
        updatedMemberFace.OriginalImageUrl.Should().Be(command.OriginalImageUrl);
        updatedMemberFace.Embedding.Should().BeEquivalentTo(command.Embedding);
        updatedMemberFace.Emotion.Should().Be(command.Emotion);
        updatedMemberFace.EmotionConfidence.Should().Be(command.EmotionConfidence);
        updatedMemberFace.IsVectorDbSynced.Should().Be(command.IsVectorDbSynced);
        updatedMemberFace.VectorDbId.Should().Be(command.VectorDbId);
        updatedMemberFace.BoundingBox.X.Should().Be(command.BoundingBox.X);
        updatedMemberFace.BoundingBox.Y.Should().Be(command.BoundingBox.Y);
    }

    [Fact]
    public async Task UpdateMemberFace_ShouldReturnFailure_WhenMemberFaceNotFound()
    {
        // Arrange
        var command = new UpdateMemberFaceCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            MemberId = Guid.NewGuid(),
            FaceId = "nonExistent",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 1, Y = 1, Width = 1, Height = 1 },
            Confidence = 0.5
        };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task UpdateMemberFace_ShouldReturnFailure_WhenUnauthorized()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Unauthorized

        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var existingMemberFace = await SeedMemberFace(family, member);

        var command = new UpdateMemberFaceCommand
        {
            Id = existingMemberFace.Id,
            MemberId = member.Id,
            FaceId = "face123",
            BoundingBox = new backend.Application.Faces.Common.BoundingBoxDto { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
        };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
