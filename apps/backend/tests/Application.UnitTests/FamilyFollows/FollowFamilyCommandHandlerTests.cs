using backend.Application.FamilyFollows.Commands.FollowFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class FollowFamilyCommandHandlerTests : TestBase
{
    // Test thành công khi tạo một theo dõi gia đình mới với các tùy chọn mặc định nếu không có tùy chọn nào được cung cấp.
    [Fact]
    public async Task Handle_GivenNoPreferences_ShouldCreateFamilyFollowWithDefaultPreferences()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family", "TF001", null, null, "Public", userId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new FollowFamilyCommand { FamilyId = family.Id };
        var handler = new FollowFamilyCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var familyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.Id == result.Value);

        familyFollow.Should().NotBeNull();
        familyFollow!.UserId.Should().Be(userId);
        familyFollow.FamilyId.Should().Be(family.Id);
        familyFollow.IsFollowing.Should().BeTrue();
        familyFollow.NotifyBirthday.Should().BeTrue();
        familyFollow.NotifyDeathAnniversary.Should().BeTrue();
        familyFollow.NotifyEvent.Should().BeTrue();

        _mockDomainEventDispatcher.Verify(
            x => x.DispatchEvents(It.Is<List<BaseEvent>>(events =>
                events.Any(e => e.GetType() == typeof(FamilyFollowCreatedEvent)))), Times.Once);
    }

    // Test thành công khi tạo một theo dõi gia đình mới với các tùy chọn được chỉ định.
    [Fact]
    public async Task Handle_GivenPreferences_ShouldCreateFamilyFollowWithSpecifiedPreferences()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 2", "TF002", null, null, "Public", userId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new FollowFamilyCommand
        {
            FamilyId = family.Id,
            NotifyBirthday = true,
            NotifyDeathAnniversary = false
        };
        var handler = new FollowFamilyCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var familyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.Id == result.Value);

        familyFollow.Should().NotBeNull();
        familyFollow!.UserId.Should().Be(userId);
        familyFollow.FamilyId.Should().Be(family.Id);
        familyFollow.IsFollowing.Should().BeTrue();
        familyFollow.NotifyBirthday.Should().BeTrue();
        familyFollow.NotifyDeathAnniversary.Should().BeFalse();
        familyFollow.NotifyEvent.Should().BeTrue();
    }

    // Test thất bại khi gia đình không tồn tại.
    [Fact]
    public async Task Handle_GivenNonExistentFamily_ShouldReturnFailure()
    {
        // Arrange
        var command = new FollowFamilyCommand { FamilyId = Guid.NewGuid() }; // Non-existent FamilyId
        var handler = new FollowFamilyCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Family with ID");
    }

    // Test thất bại khi người dùng đã theo dõi gia đình.
    [Fact]
    public async Task Handle_GivenAlreadyFollowing_ShouldReturnFailure()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 3", "TF003", null, null, "Public", userId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Create an existing follow
        var existingFollow = FamilyFollow.Create(userId, family.Id);
        await _context.FamilyFollows.AddAsync(existingFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new FollowFamilyCommand { FamilyId = family.Id };
        var handler = new FollowFamilyCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User {userId} is already following family {family.Id}");
    }

    // Test trường hợp người dùng là thành viên của gia đình (không nên gây lỗi ở đây mà logic sẽ nằm ở Unfollow)
    [Fact]
    public async Task Handle_GivenUserIsFamilyMember_ShouldStillCreateFamilyFollow()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 4", "TF004", null, null, "Public", userId);
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new FollowFamilyCommand { FamilyId = family.Id };
        var handler = new FollowFamilyCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var familyFollow = await _context.FamilyFollows.FirstOrDefaultAsync(ff => ff.Id == result.Value);
        familyFollow.Should().NotBeNull();
        familyFollow!.UserId.Should().Be(userId);
        familyFollow.FamilyId.Should().Be(family.Id);
    }
}
