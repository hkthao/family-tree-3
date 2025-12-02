using backend.Application.Members.Queries.SearchMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.SearchMembers;

public class SearchMembersQueryHandlerTests : TestBase
{
    public SearchMembersQueryHandlerTests()
    {
        // No need to initialize _mockAuthorizationService and _mockCurrentUser here, as TestBase does it.
        // We might want to override their behavior in specific tests if needed.
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfMembers()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var familyId3 = Guid.NewGuid();

        _context.Families.AddRange(
            new Family { Id = familyId1, Name = "Family1", Code = "F1" },
            new Family { Id = familyId2, Name = "Family2", Code = "F2" },
            new Family { Id = familyId3, Name = "Family3", Code = "F3" }
        );

        _context.Members.AddRange(
            new Member("John", "Doe", "JD1", familyId1),
            new Member("Jane", "Doe", "JD2", familyId2),
            new Member("Peter", "Jones", "PJ", familyId3)
        );
        await _context.SaveChangesAsync();

        var handler = new SearchMembersQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new SearchMembersQuery { Page = 1, ItemsPerPage = 2, FamilyId = familyId1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1); // Only members from familyId1
        result.Value.TotalItems.Should().Be(1);
    }
}
