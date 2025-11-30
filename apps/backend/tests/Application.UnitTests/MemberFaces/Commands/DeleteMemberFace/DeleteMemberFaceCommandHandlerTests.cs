using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<DeleteMemberFaceCommandHandler>> _loggerMock;

    public DeleteMemberFaceCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<DeleteMemberFaceCommandHandler>>();
        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private DeleteMemberFaceCommandHandler CreateHandler()
    {
        return new DeleteMemberFaceCommandHandler(_context, _authorizationServiceMock.Object, _loggerMock.Object);
    }

    private async Task<MemberFace> SeedMemberFace(Family family, Member member)
    {
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        var memberFace = new MemberFace
        {
            MemberId = member.Id,
            FaceId = "testFaceId",
            BoundingBox = new BoundingBox { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.9,
            ThumbnailUrl = "http://thumbnail.url",
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "neutral",
            EmotionConfidence = 0.5,
            IsVectorDbSynced = false,
            VectorDbId = "testVectorDbId"
        };
        await _context.MemberFaces.AddAsync(memberFace);
        await _context.SaveChangesAsync();
        return memberFace;
    }

    [Fact]
    public async Task DeleteMemberFace_ShouldDeleteMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var existingMemberFace = await SeedMemberFace(family, member);

        var command = new DeleteMemberFaceCommand { Id = existingMemberFace.Id };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedMemberFace = await _context.MemberFaces.FindAsync(existingMemberFace.Id);
        deletedMemberFace.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMemberFace_ShouldReturnFailure_WhenMemberFaceNotFound()
    {
        // Arrange
        var command = new DeleteMemberFaceCommand { Id = Guid.NewGuid() }; // Non-existent ID
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task DeleteMemberFace_ShouldReturnFailure_WhenUnauthorized()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Unauthorized

        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var existingMemberFace = await SeedMemberFace(family, member);

        var command = new DeleteMemberFaceCommand { Id = existingMemberFace.Id };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
