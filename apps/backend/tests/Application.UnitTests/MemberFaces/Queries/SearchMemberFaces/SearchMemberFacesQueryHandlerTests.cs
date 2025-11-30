using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.SearchMemberFaces;
using backend.Application.MemberFaces.Queries.MemberFaces;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediatR;
using System.Linq;

namespace backend.Application.UnitTests.MemberFaces.Queries.SearchMemberFaces;

public class SearchMemberFacesQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<SearchMemberFacesQueryHandler>> _searchLoggerMock;

    public SearchMemberFacesQueryHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _searchLoggerMock = new Mock<ILogger<SearchMemberFacesQueryHandler>>();

        // Default authorization setup for tests
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    private SearchMemberFacesQueryHandler CreateSearchHandler()
    {
        return new SearchMemberFacesQueryHandler(_context, _authorizationServiceMock.Object);
    }

    private async Task SeedData(Family family, Member member, List<MemberFace> memberFaces)
    {
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.MemberFaces.AddRangeAsync(memberFaces);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldReturnPaginatedList_WhenAuthorizedAndNoFilters()
    {
        // Arrange
        var family = new Family { Name = "Family A", Code = "FA" };
        var member = new Member(Guid.NewGuid(), "Member One", "MO", "MO", family.Id, family);
        var memberFaces = new List<MemberFace>
        {
            new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "face1", Confidence = 0.9, BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.1}, Created = DateTime.UtcNow.AddDays(-2) },
            new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "face2", Confidence = 0.8, BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.2}, Created = DateTime.UtcNow.AddDays(-1) },
        };
        await SeedData(family, member, memberFaces);

        var query = new SearchMemberFacesQuery { Page = 1, ItemsPerPage = 10 };
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldReturnFilteredByFamilyId_WhenAuthorized()
    {
        // Arrange
        var familyA = new Family { Name = "Family A", Code = "FA" };
        var memberA = new Member(Guid.NewGuid(), "Member A", "MA", "MA", familyA.Id, familyA);
        var faceA = new MemberFace { Id = Guid.NewGuid(), MemberId = memberA.Id, FaceId = "faceA", BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.1} };
        
        var familyB = new Family { Name = "Family B", Code = "FB" };
        var memberB = new Member(Guid.NewGuid(), "Member B", "MB", "MB", familyB.Id, familyB);
        var faceB = new MemberFace { Id = Guid.NewGuid(), MemberId = memberB.Id, FaceId = "faceB", BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.2} };

        await _context.Families.AddAsync(familyA);
        await _context.Members.AddAsync(memberA);
        await _context.MemberFaces.AddAsync(faceA);
        
        await _context.Families.AddAsync(familyB);
        await _context.Members.AddAsync(memberB);
        await _context.MemberFaces.AddAsync(faceB);
        await _context.SaveChangesAsync();

        var query = new SearchMemberFacesQuery { FamilyId = familyA.Id };
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(mf => mf.Id == faceA.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldReturnFilteredByMemberId_WhenAuthorized()
    {
        // Arrange
        var family = new Family { Name = "Family A", Code = "FA" };
        var member1 = new Member(Guid.NewGuid(), "Member One", "MO", "MO", family.Id, family);
        var face1 = new MemberFace { Id = Guid.NewGuid(), MemberId = member1.Id, FaceId = "face1", BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.1} };
        
        var member2 = new Member(Guid.NewGuid(), "Member Two", "MT", "MT", family.Id, family);
        var face2 = new MemberFace { Id = Guid.NewGuid(), MemberId = member2.Id, FaceId = "face2", BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.2} };

        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member1);
        await _context.MemberFaces.AddAsync(face1);
        
        await _context.Members.AddAsync(member2);
        await _context.MemberFaces.AddAsync(face2);
        await _context.SaveChangesAsync();

        var query = new SearchMemberFacesQuery { MemberId = member1.Id };
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle(mf => mf.Id == face1.Id);
        result.Value.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldReturnEmptyList_WhenNoMatchingFaces()
    {
        // Arrange
        var family = new Family { Name = "Family A", Code = "FA" };
        var member = new Member(Guid.NewGuid(), "Member One", "MO", "MO", family.Id, family);
        await _context.Families.AddAsync(family);
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var query = new SearchMemberFacesQuery { MemberId = Guid.NewGuid() }; // Non-existent member
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldReturnFailure_WhenUnauthorizedForFamilyFilter()
    {
        // Arrange
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Unauthorized

        var family = new Family { Name = "Family A", Code = "FA" };
        var member = new Member(Guid.NewGuid(), "Member One", "MO", "MO", family.Id, family);
        var memberFace = new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "face1", BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.1} };
        await SeedData(family, member, new List<MemberFace>{memberFace});

        var query = new SearchMemberFacesQuery { FamilyId = family.Id };
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task SearchMemberFaces_ShouldApplySortingAndPagination()
    {
        // Arrange
        var family = new Family { Name = "Family A", Code = "FA" };
        var member = new Member(Guid.NewGuid(), "Member One", "MO", "MO", family.Id, family);
        var memberFaces = new List<MemberFace>
        {
            new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "faceC", Confidence = 0.7, BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.3}, Created = DateTime.UtcNow.AddDays(-3) },
            new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "faceA", Confidence = 0.9, BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.1}, Created = DateTime.UtcNow.AddDays(-1) },
            new MemberFace { Id = Guid.NewGuid(), MemberId = member.Id, FaceId = "faceB", Confidence = 0.8, BoundingBox = new BoundingBox{X=1,Y=1,Width=1,Height=1}, Embedding = new List<double>{0.2}, Created = DateTime.UtcNow.AddDays(-2) },
        };
        await SeedData(family, member, memberFaces);

        var query = new SearchMemberFacesQuery { Page = 1, ItemsPerPage = 2, SortBy = "FaceId", SortOrder = "asc" };
        var handler = CreateSearchHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Items.First().FaceId.Should().Be("faceA");
        result.Value.Items.Last().FaceId.Should().Be("faceB");
    }
}
