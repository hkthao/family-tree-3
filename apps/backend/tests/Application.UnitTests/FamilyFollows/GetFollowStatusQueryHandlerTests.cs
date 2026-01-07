using backend.Application.FamilyFollows.Queries.GetFollowStatus;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class GetFollowStatusQueryHandlerTests : TestBase
{
    // Test thành công khi trả về FamilyFollowDto nếu người dùng đang theo dõi gia đình được chỉ định.
    [Fact]
    public async Task Handle_ShouldReturnFamilyFollowDto_WhenUserIsFollowingFamily()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Followed Family", "FF001", null, null, "Public", Guid.NewGuid());
        await _context.Families.AddAsync(family);

        var familyFollow = FamilyFollow.Create(userId, family.Id);
        familyFollow.NotifyBirthday = true;
        await _context.FamilyFollows.AddAsync(familyFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFollowStatusQuery { FamilyId = family.Id };
        var handler = new GetFollowStatusQueryHandler(_context, _mapper, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(family.Id);
        result.Value.UserId.Should().Be(userId);
        result.Value.IsFollowing.Should().BeTrue();
        result.Value.NotifyBirthday.Should().BeTrue();
        result.Value.NotifyDeathAnniversary.Should().BeTrue(); // Expected True due to FamilyFollow.Create default
        result.Value.NotifyEvent.Should().BeTrue(); // Expected True due to FamilyFollow.Create default
    }

    // Test thất bại khi người dùng không theo dõi gia đình được chỉ định.
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotFollowingFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Not followed
        var query = new GetFollowStatusQuery { FamilyId = familyId };
        var handler = new GetFollowStatusQueryHandler(_context, _mapper, _mockUser.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("is not following family");
    }
}
