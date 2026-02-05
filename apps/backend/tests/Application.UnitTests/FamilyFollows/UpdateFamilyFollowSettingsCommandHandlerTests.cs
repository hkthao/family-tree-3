using backend.Application.FamilyFollows.Commands.UpdateFamilyFollowSettings;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // Added for BaseEvent
using backend.Domain.Entities;
using backend.Domain.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyFollows;

public class UpdateFamilyFollowSettingsCommandHandlerTests : TestBase
{
    // Test thành công khi cập nhật tùy chọn hiện có và thêm tùy chọn mới.
    [Fact]
    public async Task Handle_ShouldUpdateExistingAndAddNewPreferences()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 1", "TF001", null, null, "Public", userId);
        await _context.Families.AddAsync(family);

        var familyFollow = FamilyFollow.Create(userId, family.Id);
        familyFollow.NotifyBirthday = true;
        familyFollow.NotifyEvent = false;
        familyFollow.NotifyDeathAnniversary = true; // Explicitly set for clarity
        await _context.FamilyFollows.AddAsync(familyFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateFamilyFollowSettingsCommand
        {
            FamilyId = family.Id,
            NotifyBirthday = false, // Update existing
            NotifyDeathAnniversary = true, // Add new
            NotifyEvent = false // No change
        };
        var handler = new UpdateFamilyFollowSettingsCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamilyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.Id == familyFollow.Id);

        updatedFamilyFollow.Should().NotBeNull();
        updatedFamilyFollow!.NotifyBirthday.Should().BeFalse();
        updatedFamilyFollow.NotifyDeathAnniversary.Should().BeTrue();
        updatedFamilyFollow.NotifyEvent.Should().BeFalse();

        _mockDomainEventDispatcher.Verify(
            x => x.DispatchEvents(It.Is<List<BaseEvent>>(events =>
                events.Any(e => e.GetType() == typeof(FamilyFollowSettingsUpdatedEvent)))), Times.Once);
    }

    // Test thành công khi xóa các tùy chọn không được chỉ định trong lệnh cập nhật.
    [Fact]
    public async Task Handle_ShouldRemoveUnspecifiedPreferences()
    {
        // Arrange
        var userId = _mockUser.Object.UserId;
        var family = Family.Create("Test Family 2", "TF002", null, null, "Public", userId);
        await _context.Families.AddAsync(family);

        var familyFollow = FamilyFollow.Create(userId, family.Id);
        familyFollow.NotifyBirthday = true;
        familyFollow.NotifyEvent = false;
        familyFollow.NotifyDeathAnniversary = true;
        await _context.FamilyFollows.AddAsync(familyFollow);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateFamilyFollowSettingsCommand
        {
            FamilyId = family.Id,
            NotifyBirthday = true, // Only one preference specified
            NotifyEvent = false,
            NotifyDeathAnniversary = false
        };
        var handler = new UpdateFamilyFollowSettingsCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamilyFollow = await _context.FamilyFollows
            .FirstOrDefaultAsync(ff => ff.Id == familyFollow.Id);

        updatedFamilyFollow.Should().NotBeNull();
        updatedFamilyFollow!.NotifyBirthday.Should().BeTrue();
        updatedFamilyFollow.NotifyEvent.Should().BeFalse();
        updatedFamilyFollow.NotifyDeathAnniversary.Should().BeFalse();
    }

    // Test thất bại khi người dùng không theo dõi gia đình.
    [Fact]
    public async Task Handle_GivenNotFollowing_ShouldReturnFailure()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Not followed
        var command = new UpdateFamilyFollowSettingsCommand
        {
            FamilyId = familyId,
            NotifyBirthday = true,
            NotifyEvent = false,
            NotifyDeathAnniversary = true
        };
        var handler = new UpdateFamilyFollowSettingsCommandHandler(_context, _mockUser.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("is not following family");
    }
}
