using backend.Application.FamilyFollows.Queries.GetUserFollowedFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class GetUserFollowedFamiliesQueryHandlerTests : TestBase
{
    // Test thành công khi trả về danh sách các gia đình mà người dùng hiện tại đang theo dõi.
    [Fact]
    public async Task Handle_ShouldReturnUserFollowedFamilies()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family1 = Family.Create("Family 1", "F1", null, null, "Public", Guid.NewGuid());
        var family2 = Family.Create("Family 2", "F2", null, null, "Public", Guid.NewGuid());
        var family3 = Family.Create("Family 3", "F3", null, null, "Public", Guid.NewGuid());
        await _context.Families.AddRangeAsync(family1, family2, family3);

        var follow1 = FamilyFollow.Create(userId, family1.Id);
        follow1.NotifyBirthday = true;
        var follow2 = FamilyFollow.Create(userId, family2.Id);
        follow2.NotifyEvent = true;
        // Another user follows family3, should not be returned
        var anotherUserFollow = FamilyFollow.Create(Guid.NewGuid(), family3.Id);

        await _context.FamilyFollows.AddRangeAsync(follow1, follow2, anotherUserFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUserFollowedFamiliesQuery();
        var handler = new GetUserFollowedFamiliesQueryHandler(_context, _mapper, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(ff => ff.FamilyId == family1.Id);
        result.Value.Should().Contain(ff => ff.FamilyId == family2.Id);
        var family1Follow = result.Value!.FirstOrDefault(ff => ff.FamilyId == family1.Id); // Added null-forgiving operator
        family1Follow.Should().NotBeNull();
        family1Follow!.NotifyBirthday.Should().BeTrue();
        family1Follow.NotifyEvent.Should().BeTrue();
    }

    // Test thành công khi trả về danh sách rỗng nếu người dùng không theo dõi gia đình nào.
    [Fact]
    public async Task Handle_ShouldReturnEmptyListIfNoFamiliesFollowed()
    {
        // Arrange
        var query = new GetUserFollowedFamiliesQuery();
        var handler = new GetUserFollowedFamiliesQueryHandler(_context, _mapper, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
