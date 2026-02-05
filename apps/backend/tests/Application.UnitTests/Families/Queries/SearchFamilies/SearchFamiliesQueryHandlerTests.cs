using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{
    private readonly SearchFamiliesQueryHandler _handler; // NEW
    private readonly Mock<IPrivacyService> _mockPrivacyService;
    private readonly Mock<ICurrentUser> _mockCurrentUser; // NEW

    public SearchFamiliesQueryHandlerTests()
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

        // Setup mock user and authorization service for the handler
        _mockUser.Setup(c => c.UserId).Returns(TestUserId); // Ensure this is setup in TestBase
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true); // Ensure this is setup in TestBase
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Default to non-admin

        _handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamiliesWhenUserIsInFamilyUsers()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId); // Ensure mock is updated for this test
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var otherUserId = Guid.NewGuid();
        var familyCreatedByOther = new Family { Id = Guid.NewGuid(), Name = "Family By Other", Code = "FBO", CreatedBy = otherUserId.ToString(), Visibility = FamilyVisibility.Private.ToString() };

        // Create and add FamilyUser directly to context, linking it to the family
        var familyUser = new FamilyUser(familyCreatedByOther.Id, authenticatedUserId, FamilyRole.Viewer);

        _context.Families.Add(familyCreatedByOther);
        _context.FamilyUsers.Add(familyUser);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Family By Other");
    }


    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfFamilies_WhenCalled()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = authenticatedUserId.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2", CreatedBy = authenticatedUserId.ToString() };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 2 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Items.First().Name.Should().Be("Family 1");
        result.Value.Items.Last().Name.Should().Be("Family 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenSearchQueryIsProvided()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family Alpha", Description = "Description for Alpha", Code = "FA", CreatedBy = authenticatedUserId.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family Beta", Description = "Description for Beta", Code = "FB", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { SearchQuery = "Alpha", Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Family Alpha");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenVisibilityIsProvided()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family Public", Visibility = "Public", Code = "FP", CreatedBy = authenticatedUserId.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family Private", Visibility = "Private", Code = "FPR", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Visibility = "Public", Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Family Public");
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderedFamilies_WhenSortByAndSortOrderAreProvided()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family C", Code = "FC", CreatedBy = authenticatedUserId.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA", CreatedBy = authenticatedUserId.ToString() };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { SortBy = "Name", SortOrder = "asc", Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items.First().Name.Should().Be("Family A");
    }

    [Fact]
    public async Task Handle_ShouldReturnFamiliesCreatedByAuthenticatedUserAndWhereUserIsAMember()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var otherUserId = Guid.NewGuid();
        var familyCreatedByAuthenticatedUser = new Family { Id = Guid.NewGuid(), Name = "My Created Family", Code = "MYF", CreatedBy = authenticatedUserId.ToString() };
        var familyCreatedByOtherUser = new Family { Id = Guid.NewGuid(), Name = "Other User's Family", Code = "OTF", CreatedBy = otherUserId.ToString() };

        // Add authenticated user as a member to the family created by another user
        var familyUserEntry = new FamilyUser(familyCreatedByOtherUser.Id, authenticatedUserId, FamilyRole.Viewer);

        _context.Families.AddRange(familyCreatedByAuthenticatedUser, familyCreatedByOtherUser);
        _context.FamilyUsers.Add(familyUserEntry);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(f => f.Name == "My Created Family");
        result.Value.Items.Should().Contain(f => f.Name == "Other User's Family");
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFamilies_WhenUserIsAdmin()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var adminUserId = TestUserId; // Use TestUserId
        _mockCurrentUser.Setup(c => c.UserId).Returns(adminUserId);
        _mockCurrentUser.Setup(c => c.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Simulate admin user

        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockCurrentUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Admin Family 1", Code = "ADM1", CreatedBy = Guid.NewGuid().ToString(), Visibility = FamilyVisibility.Private.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Admin Family 2", Code = "ADM2", CreatedBy = Guid.NewGuid().ToString(), Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2); // Admin should see all families
        result.Value.Items.Should().Contain(f => f.Name == "Admin Family 1");
        result.Value.Items.Should().Contain(f => f.Name == "Admin Family 2");
    }

    // NEW TESTS FOR ISFOLLOWING FILTER

    [Fact]
    public async Task Handle_IsFollowingTrue_ShouldReturnFollowedFamiliesForAuthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Private.ToString() };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };

        _context.Families.AddRange(family1, family2, family3);
        _context.FamilyFollows.Add(FamilyFollow.Create(TestUserId, family1.Id));
        _context.FamilyFollows.Add(FamilyFollow.Create(TestUserId, family2.Id));
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { IsFollowing = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(f => f.Id == family1.Id);
        result.Value.Items.Should().Contain(f => f.Id == family2.Id);
    }

    [Fact]
    public async Task Handle_IsFollowingFalse_ShouldReturnUnfollowedFamiliesForAuthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Private.ToString() };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };

        _context.Families.AddRange(family1, family2, family3);
        _context.FamilyFollows.Add(FamilyFollow.Create(TestUserId, family1.Id));
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { IsFollowing = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(f => f.Id == family2.Id);
        result.Value.Items.Should().Contain(f => f.Id == family3.Id);
    }

    [Fact]
    public async Task Handle_IsFollowingTrue_ShouldReturnEmptyListForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(family1);
        _context.FamilyFollows.Add(FamilyFollow.Create(TestUserId, family1.Id)); // Followed by some user, but current is unauthenticated
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { IsFollowing = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_IsFollowingFalse_ShouldReturnAllFamiliesForUnauthenticatedUser()
    {
        // Arrange
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Public.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2", CreatedBy = TestUserId.ToString(), Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { IsFollowing = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2); // All accessible families are considered "not followed"
    }
}
