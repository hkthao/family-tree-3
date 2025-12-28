using backend.Domain.Entities;
using FluentAssertions; // Add this
using Xunit; // Make sure Xunit is imported

namespace backend.Domain.UnitTests;

public class FamilyTests
{
    [Fact]
    public void Create_ShouldInitializeFamilyWithDefaultFamilyLimitConfiguration()
    {
        // Arrange
        var familyName = "Test Family";
        var familyCode = "TF001";
        var creatorUserId = Guid.NewGuid();

        // Act
        var family = Family.Create(familyName, familyCode, null, null, "Private", creatorUserId);

        // Assert
        family.Should().NotBeNull();
        family.FamilyLimitConfiguration.Should().NotBeNull();
        family.FamilyLimitConfiguration!.FamilyId.Should().Be(family.Id);
        family.FamilyLimitConfiguration!.MaxMembers.Should().Be(5000);
        family.FamilyLimitConfiguration!.MaxStorageMb.Should().Be(2048);
        family.FamilyLimitConfiguration!.AiChatMonthlyLimit.Should().Be(100);
    }

    [Fact]
    public void UpdateFamilyConfiguration_ShouldUpdateExistingConfiguration()
    {
        // Arrange
        var familyName = "Test Family";
        var familyCode = "TF001";
        var creatorUserId = Guid.NewGuid();
        var family = Family.Create(familyName, familyCode, null, null, "Private", creatorUserId);

        var newMaxMembers = 100;
        var newMaxStorageMb = 2048;

        // Act
        family.UpdateFamilyConfiguration(newMaxMembers, newMaxStorageMb, 0);

        // Assert
        family.FamilyLimitConfiguration.Should().NotBeNull();
        family.FamilyLimitConfiguration!.MaxMembers.Should().Be(newMaxMembers);
        family.FamilyLimitConfiguration!.MaxStorageMb.Should().Be(newMaxStorageMb);
    }

    [Fact]
    public void UpdateFamilyConfiguration_ShouldThrowArgumentException_ForInvalidValues()
    {
        // Arrange
        var familyName = "Test Family";
        var familyCode = "TF001";
        var creatorUserId = Guid.NewGuid();
        var family = Family.Create(familyName, familyCode, null, null, "Private", creatorUserId);

        var invalidMaxMembers = -10;
        var validMaxStorageMb = 1024;

        // Act
        Action act = () => family.UpdateFamilyConfiguration(invalidMaxMembers, validMaxStorageMb, 0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
