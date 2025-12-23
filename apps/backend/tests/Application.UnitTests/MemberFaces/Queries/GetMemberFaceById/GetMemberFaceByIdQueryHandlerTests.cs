using backend.Application.Common.Constants;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Queries.GetMemberFaceById;

public class GetMemberFaceByIdQueryHandlerTests : TestBase

{

    public GetMemberFaceByIdQueryHandlerTests()

    {

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Default non-admin

        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);

    }



    private GetMemberFaceByIdQueryHandler CreateGetByIdHandler()

    {

        return new GetMemberFaceByIdQueryHandler(_context, _mockUser.Object, _mockAuthorizationService.Object, _mapper);

    }

    [Fact]
    public async Task GetMemberFaceById_ShouldReturnMemberFace_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Test Family", Code = "TF" };
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var memberFace = new MemberFace
        {
            MemberId = member.Id,
            FaceId = "face123",
            BoundingBox = new BoundingBox { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99,
            ThumbnailUrl = "http://thumbnail.url",
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = true
        };
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.MemberFaces.AddAsync(memberFace);
        await _context.SaveChangesAsync();

        var query = new GetMemberFaceByIdQuery { Id = memberFace.Id };
        var handler = CreateGetByIdHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberFace.Id);
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.FaceId.Should().Be(memberFace.FaceId);
        result.Value.FamilyId.Should().Be(family.Id);
    }

    [Fact]
    public async Task GetMemberFaceById_ShouldReturnFailure_WhenMemberFaceNotFound()
    {
        // Arrange
        var query = new GetMemberFaceByIdQuery { Id = Guid.NewGuid() }; // Non-existent ID
        var handler = CreateGetByIdHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task GetMemberFaceById_ShouldReturnFailure_WhenUnauthorized()
    {

        var family = new Family { Name = "Test Family", Code = "TF", CreatedBy = Guid.NewGuid().ToString() }; // Family not created by unauthorizedUser
        var member = new Member(Guid.NewGuid(), "John", "Doe", "JD", family.Id, family);
        var memberFace = new MemberFace
        {
            MemberId = member.Id,
            FaceId = "face123",
            BoundingBox = new BoundingBox { X = 10, Y = 20, Width = 50, Height = 60 },
            Confidence = 0.99f,
            ThumbnailUrl = "http://thumbnail.url",
            OriginalImageUrl = "http://original.url",
            Embedding = new List<double> { 0.1, 0.2, 0.3 },
            Emotion = "happy",
            EmotionConfidence = 0.95,
            IsVectorDbSynced = true
        };
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.MemberFaces.AddAsync(memberFace);
        await _context.SaveChangesAsync();

        // Arrange
        var unauthorizedUserId = Guid.NewGuid(); // A user ID that does not have access
        _mockUser.Setup(x => x.UserId).Returns(unauthorizedUserId); // Unauthorized user
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Ensure not admin

        var query = new GetMemberFaceByIdQuery { Id = memberFace.Id };
        var handler = CreateGetByIdHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound); // Should be NotFound because the specification filters it out
    }
}
