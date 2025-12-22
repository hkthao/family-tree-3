using backend.Domain.Entities;
using FluentAssertions; // Add this
using Xunit; // Make sure Xunit is imported
using System; // Make sure System is imported

namespace backend.Domain.UnitTests;

public class FamilyLimitConfigurationTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValuesAndFamilyId()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Act
        var config = new FamilyLimitConfiguration(familyId);

        // Assert
        config.Should().NotBeNull();
        config.FamilyId.Should().Be(familyId);
        config.MaxMembers.Should().Be(50);
        config.MaxStorageMb.Should().Be(1024);
        config.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // FluentAssertions for DateTime
        config.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(100, 2048)]
    [InlineData(1, 1)]
    public void Update_ShouldUpdateValuesCorrectly(int newMaxMembers, int newMaxStorageMb)
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var config = new FamilyLimitConfiguration(familyId);

        // Act
        config.Update(newMaxMembers, newMaxStorageMb, 0);

        // Assert
        config.MaxMembers.Should().Be(newMaxMembers);
        config.MaxStorageMb.Should().Be(newMaxStorageMb);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-10, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -10)]
    public void Update_ShouldThrowArgumentException_ForInvalidValues(int newMaxMembers, int newMaxStorageMb)
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var config = new FamilyLimitConfiguration(familyId);

        // Act
        Action act = () => config.Update(newMaxMembers, newMaxStorageMb, 0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
