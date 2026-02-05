using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Domain.Entities;
using backend.Domain.Events.MemberFaces; // Added
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore; // Added for mock DbSet
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Commands.DeleteMemberFace;

public class DeleteMemberFaceCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<DeleteMemberFaceCommandHandler>> _loggerMock;
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly List<MemberFace> _memberFacesData;
    private readonly List<Family> _familiesData;
    private readonly List<Member> _membersData;

    public DeleteMemberFaceCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<DeleteMemberFaceCommandHandler>>();
        _messageBusMock = new Mock<IMessageBus>();

        _memberFacesData = new List<MemberFace>();
        _familiesData = new List<Family>();
        _membersData = new List<Member>();

        // Setup mock DbSets
        _contextMock.Setup(x => x.MemberFaces).ReturnsDbSet(_memberFacesData);
        _contextMock.Setup(x => x.Families).ReturnsDbSet(_familiesData);
        _contextMock.Setup(x => x.Members).ReturnsDbSet(_membersData);
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
        return new DeleteMemberFaceCommandHandler(_contextMock.Object, _authorizationServiceMock.Object, _loggerMock.Object, _messageBusMock.Object);
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
