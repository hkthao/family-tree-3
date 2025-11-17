using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Application.Members.Queries.SearchMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.SearchMembers;

public class SearchMembersQueryHandlerTests : TestBase
{
    private readonly Mock<IPrivacyService> _privacyServiceMock;

    public SearchMembersQueryHandlerTests()
    {
        _privacyServiceMock = new Mock<IPrivacyService>();
        // Setup mock to return the original list without filtering for tests
        _privacyServiceMock.Setup(p => p.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid familyId, CancellationToken ct) => members);
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

        var handler = new SearchMembersQueryHandler(_context, _mapper, _privacyServiceMock.Object);
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
