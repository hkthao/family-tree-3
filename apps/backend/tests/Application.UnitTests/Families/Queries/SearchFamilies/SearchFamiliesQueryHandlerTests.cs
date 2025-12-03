using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{


    public SearchFamiliesQueryHandlerTests()
    {
        // Set up authenticated user by default for most tests
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        // Default to non-admin for most tests, override in specific admin tests
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamiliesWhenUserIsInFamilyUsers()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

        var adminUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(adminUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Simulate admin user

        var handler = new SearchFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

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

}