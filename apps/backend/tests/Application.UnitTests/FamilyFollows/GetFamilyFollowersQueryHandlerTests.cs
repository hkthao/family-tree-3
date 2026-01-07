using backend.Application.FamilyFollows.Queries.GetFamilyFollowers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class GetFamilyFollowersQueryHandlerTests : TestBase
{
    // Test thành công khi trả về danh sách người dùng theo dõi một gia đình cụ thể.
    [Fact]
    public async Task Handle_ShouldReturnFamilyFollowers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Target Family", "TF001", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        await _context.Families.AddAsync(family);

        var follower1Id = Guid.NewGuid();
        var follower2Id = Guid.NewGuid();
        var follower3Id = Guid.NewGuid(); // Follows another family

        var follow1 = FamilyFollow.Create(follower1Id, familyId);
        var follow2 = FamilyFollow.Create(follower2Id, familyId);
        var follow3 = FamilyFollow.Create(follower3Id, Guid.NewGuid()); // Another family

        await _context.FamilyFollows.AddRangeAsync(follow1, follow2, follow3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFamilyFollowersQuery { FamilyId = familyId };
        var handler = new GetFamilyFollowersQueryHandler(_context, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(ff => ff.UserId == follower1Id);
        result.Value.Should().Contain(ff => ff.UserId == follower2Id);
        result.Value.Should().NotContain(ff => ff.UserId == follower3Id);
    }

    // Test thành công khi trả về danh sách rỗng nếu không có người dùng nào theo dõi gia đình được chỉ định.
    [Fact]
    public async Task Handle_ShouldReturnEmptyListIfNoFollowers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Empty Followers Family", "EF001", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFamilyFollowersQuery { FamilyId = familyId };
        var handler = new GetFamilyFollowersQueryHandler(_context, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    // Test thành công khi trả về danh sách rỗng nếu gia đình không tồn tại.
    [Fact]
    public async Task Handle_ShouldReturnEmptyListIfFamilyDoesNotExist()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new GetFamilyFollowersQuery { FamilyId = nonExistentFamilyId };
        var handler = new GetFamilyFollowersQueryHandler(_context, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
