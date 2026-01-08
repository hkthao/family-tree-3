using backend.Application.FamilyFollows.Commands.UnfollowFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class UnfollowFamilyCommandHandlerTests : TestBase
{
    // Test thành công khi người dùng bỏ theo dõi một gia đình.
    [Fact]
    public async Task Handle_GivenExistingFollow_ShouldUnfollowSuccessfully()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 1", "TF001", null, null, "Public", Guid.NewGuid()); // Make sure userId is not a member of this family
        var familyFollow = FamilyFollow.Create(userId, family.Id);
        await _context.FamilyFollows.AddAsync(familyFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UnfollowFamilyCommand { FamilyId = family.Id };
        var handler = new UnfollowFamilyCommandHandler(_context, _mockUser.Object, _mockDomainEventDispatcher.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var unfollowed = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.UserId == userId && ff.FamilyId == family.Id);
        unfollowed.Should().BeNull(); // Should be removed

        _mockDomainEventDispatcher.Verify(
            x => x.DispatchEvents(It.Is<List<BaseEvent>>(events =>
                events.Any(e => e.GetType() == typeof(FamilyFollowDeletedEvent)))), Times.Once);
    }

    // Test thất bại khi người dùng không theo dõi gia đình.
    [Fact]
    public async Task Handle_GivenNotFollowing_ShouldReturnFailure()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Not followed
        var command = new UnfollowFamilyCommand { FamilyId = familyId };
        var handler = new UnfollowFamilyCommandHandler(_context, _mockUser.Object, _mockDomainEventDispatcher.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("is not following family");
    }

    // Test thành công khi người dùng là thành viên của gia đình và đang theo dõi, nên vẫn có thể bỏ theo dõi.
    [Fact]
    public async Task Handle_GivenUserIsFamilyMemberAndFollowing_ShouldUnfollowSuccessfully()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 2", "TF002", null, null, "Public", userId);
        await _context.Families.AddAsync(family);

        // Ensure user is a member (implicitly handled by Family.Create with userId as creator)
        // And also ensure there's a follow record
        var familyFollow = FamilyFollow.Create(userId, family.Id);
        await _context.FamilyFollows.AddAsync(familyFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UnfollowFamilyCommand { FamilyId = family.Id };
        var handler = new UnfollowFamilyCommandHandler(_context, _mockUser.Object, _mockDomainEventDispatcher.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Changed to BeTrue

        var unfollowed = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.UserId == userId && ff.FamilyId == family.Id);
        unfollowed.Should().BeNull(); // Should be removed

        _mockDomainEventDispatcher.Verify(
            x => x.DispatchEvents(It.Is<List<BaseEvent>>(events =>
                events.Any(e => e.GetType() == typeof(FamilyFollowDeletedEvent)))), Times.Once);
    }
}
