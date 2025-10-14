using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests : TestBase
{
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _handler = new GetMembersQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Members()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        int countBefore = _context.Members.Count();
        var members = new List<Member>
        {
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow }
        };
        _context.AddRange(members);
        _context.SaveChanges();

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);
        // Assert
        result.Value.Should().HaveCount(countBefore + 2);
        result.Value.Should().ContainEquivalentOf(new { FullName = "Member 1" });
        result.Value.Should().ContainEquivalentOf(new { FullName = "Member 2" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await _handler.Handle(new GetMembersQuery() { FamilyId = Guid.NewGuid() }, CancellationToken.None);
        // Assert
        result.Value.Should().BeEmpty();
    }
}
