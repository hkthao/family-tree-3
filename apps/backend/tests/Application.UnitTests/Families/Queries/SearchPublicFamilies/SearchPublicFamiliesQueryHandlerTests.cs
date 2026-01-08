using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Families.Queries.SearchPublicFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchPublicFamilies;

public class SearchPublicFamiliesQueryHandlerTests : TestBase
{
    private readonly SearchPublicFamiliesQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;
    private readonly Mock<ICurrentUser> _mockCurrentUser; // NEW

    public SearchPublicFamiliesQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<FamilyDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FamilyDto> dtos, Guid familyId, CancellationToken token) => dtos);

        _mockCurrentUser = new Mock<ICurrentUser>(); // NEW
        _mockCurrentUser.Setup(x => x.UserId).Returns(TestUserId); // Use a consistent TestUserId from TestBase
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true); // Default to authenticated for most tests

        _handler = new SearchPublicFamiliesQueryHandler(_context, _mapper, _mockPrivacyService.Object, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyPublicFamilies()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family 2", Code = "PUB2", Visibility = FamilyVisibility.Public.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredPublicFamilies_WhenSearchTermProvided()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Alpha", Code = "PUB_A", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Beta", Code = "PUB_B", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family Gamma", Code = "PRIV_G", Visibility = FamilyVisibility.Private.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { SearchQuery = "Alpha" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Public Family Alpha");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPublicFamiliesExist()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatchingPublicFamilies()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Alpha", Code = "PUB_A", Visibility = FamilyVisibility.Public.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { SearchQuery = "NonExistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    // NEW TESTS FOR ISFOLLOWING FILTER

    [Fact]
    public async Task Handle_IsFollowingTrue_ShouldReturnFollowedPublicFamiliesForAuthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var publicFamily1 = new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        var publicFamily2 = new Family { Id = Guid.NewGuid(), Name = "Public Family 2", Code = "PUB2", Visibility = FamilyVisibility.Public.ToString() };
        var privateFamily = new Family { Id = Guid.NewGuid(), Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };

        _context.Families.AddRange(publicFamily1, publicFamily2, privateFamily);
        _context.FamilyFollows.Add(new FamilyFollow { FamilyId = publicFamily1.Id, UserId = TestUserId });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { IsFollowing = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Id.Should().Be(publicFamily1.Id);
    }

    [Fact]
    public async Task Handle_IsFollowingFalse_ShouldReturnUnfollowedPublicFamiliesForAuthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var publicFamily1 = new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        var publicFamily2 = new Family { Id = Guid.NewGuid(), Name = "Public Family 2", Code = "PUB2", Visibility = FamilyVisibility.Public.ToString() };
        var privateFamily = new Family { Id = Guid.NewGuid(), Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };

        _context.Families.AddRange(publicFamily1, publicFamily2, privateFamily);
        _context.FamilyFollows.Add(new FamilyFollow { FamilyId = publicFamily1.Id, UserId = TestUserId });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { IsFollowing = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Id.Should().Be(publicFamily2.Id);
    }

    [Fact]
    public async Task Handle_IsFollowingTrue_ShouldReturnEmptyListForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
        var publicFamily1 = new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily1);
        _context.FamilyFollows.Add(new FamilyFollow { FamilyId = publicFamily1.Id, UserId = TestUserId }); // Followed by some user, but current is unauthenticated
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { IsFollowing = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_IsFollowingFalse_ShouldReturnAllPublicFamiliesForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
        var publicFamily1 = new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        var publicFamily2 = new Family { Id = Guid.NewGuid(), Name = "Public Family 2", Code = "PUB2", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.AddRange(publicFamily1, publicFamily2);
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { IsFollowing = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2); // All public families are considered "not followed"
    }
}

