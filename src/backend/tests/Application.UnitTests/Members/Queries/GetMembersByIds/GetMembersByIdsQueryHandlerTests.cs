using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandlerTests : TestBase
{
    public GetMembersByIdsQueryHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectMembers_WhenGivenValidIds()
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

        // Arrange
        var member1 = new Member("John", "Doe", "JD1", familyId1);
        var member2 = new Member("Jane", "Doe", "JD2", familyId2);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var handler = new GetMembersByIdsQueryHandler(_context, _mapper);
        var query = new GetMembersByIdsQuery(new List<Guid> { member1.Id, member2.Id });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenGivenNoIds()
    {
        // Arrange
        var handler = new GetMembersByIdsQueryHandler(_context, _mapper);
        var query = new GetMembersByIdsQuery(new List<Guid>());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
