using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Domain.Entities;
using backend.Domain.Events.MemberFaces; // Added
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore; // Added
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore; // Added for mock DbSet
using Xunit;
using backend.Application.Knowledge; // NEW

namespace backend.Application.UnitTests.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<DbSet<MemberFace>> _memberFacesDbSetMock;
    private readonly Mock<DbSet<Family>> _familiesDbSetMock; // Added
    private readonly Mock<DbSet<Member>> _membersDbSetMock; // Added
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<DeleteMemberFaceCommandHandler>> _loggerMock;
    private readonly Mock<IKnowledgeService> _knowledgeServiceMock; // NEW
    private readonly List<MemberFace> _memberFacesData;
    private readonly List<Family> _familiesData; // Added
    private readonly List<Member> _membersData; // Added

    public DeleteMemberFaceCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _memberFacesDbSetMock = new Mock<DbSet<MemberFace>>();
        _familiesDbSetMock = new Mock<DbSet<Family>>(); // Initialized
        _membersDbSetMock = new Mock<DbSet<Member>>(); // Initialized
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<DeleteMemberFaceCommandHandler>>();
        _knowledgeServiceMock = new Mock<IKnowledgeService>(); // NEW

        _memberFacesData = new List<MemberFace>();
        _familiesData = new List<Family>(); // Initialized
        _membersData = new List<Member>(); // Initialized

        // Setup mock DbSets
        _contextMock.Setup(x => x.MemberFaces).ReturnsDbSet(_memberFacesData);
        _contextMock.Setup(x => x.Families).ReturnsDbSet(_familiesData); // Setup mock for Families
        _contextMock.Setup(x => x.Members).ReturnsDbSet(_membersData); // Setup mock for Members
        _contextMock.Setup(x => x.MemberFaces.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] ids) => _memberFacesData.FirstOrDefault(mf => mf.Id == (Guid)ids[0]));

        // Setup Remove for MemberFaces to simulate removing from the list and adding DomainEvent
        _contextMock.Setup(x => x.MemberFaces.Remove(It.IsAny<MemberFace>()))
            .Callback<MemberFace>(mf =>
            {
                _memberFacesData.Remove(mf);
            });

        // Mock SaveChangesAsync to do nothing as we are testing the command handler's logic
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private DeleteMemberFaceCommandHandler CreateHandler()
    {
        return new DeleteMemberFaceCommandHandler(_contextMock.Object, _authorizationServiceMock.Object, _loggerMock.Object, _knowledgeServiceMock.Object);
    }

    private MemberFace SeedMemberFace(Family family, Member member)
    {
        _familiesData.Add(family);
        _membersData.Add(member);
        var memberFace = new MemberFace
        {
            Id = Guid.NewGuid(), // Cần tạo Id rõ ràng vì không dùng DbContext
            MemberId = member.Id,
            Member = member, // Gán đối tượng Member
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
        _memberFacesData.Add(memberFace);
        return memberFace;
    }

    [Fact]
    public async Task DeleteMemberFace_ShouldDeleteMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var existingMemberFace = SeedMemberFace(family, member);

        var command = new DeleteMemberFaceCommand { Id = existingMemberFace.Id };
        var handler = CreateHandler();

        _knowledgeServiceMock.Setup(x => x.DeleteMemberFaceData(family.Id, existingMemberFace.Id))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedMemberFace = _memberFacesData.FirstOrDefault(mf => mf.Id == existingMemberFace.Id);
        deletedMemberFace.Should().BeNull();

        existingMemberFace.DomainEvents.Should().ContainSingle(e => e is MemberFaceDeletedEvent);
        var domainEvent = existingMemberFace.DomainEvents.OfType<MemberFaceDeletedEvent>().Single();
        domainEvent.MemberFace.Should().Be(existingMemberFace);
        domainEvent.VectorDbId.Should().Be(existingMemberFace.VectorDbId);

        _knowledgeServiceMock.Verify(x => x.DeleteMemberFaceData(family.Id, existingMemberFace.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteMemberFace_ShouldReturnFailure_WhenMemberFaceNotFound()
    {
        // Arrange - No need to seed, as ID is non-existent
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
        var existingMemberFace = SeedMemberFace(family, member);

        var command = new DeleteMemberFaceCommand { Id = existingMemberFace.Id };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        existingMemberFace.DomainEvents.Should().NotContain(e => e is MemberFaceDeletedEvent);
    }
}
