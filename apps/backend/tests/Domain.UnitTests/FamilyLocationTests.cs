using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests;

public class FamilyLocationTests
{
    [Fact]
    public void FamilyLocation_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var name = "Test Location";
        var description = "A description for the test location";
        var latitude = 10.0;
        var longitude = 20.0;
        var address = "123 Test St";
                    var locationType = LocationType.Homeland;        var accuracy = LocationAccuracy.Exact;
        var source = LocationSource.UserSelected;

        // Act
        var familyLocation = new FamilyLocation
        {
            FamilyId = familyId,
            Name = name,
            Description = description,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
            LocationType = locationType,
            Accuracy = accuracy,
            Source = source
        };

        // Assert
        familyLocation.FamilyId.Should().Be(familyId);
        familyLocation.Name.Should().Be(name);
        familyLocation.Description.Should().Be(description);
        familyLocation.Latitude.Should().Be(latitude);
        familyLocation.Longitude.Should().Be(longitude);
        familyLocation.Address.Should().Be(address);
        familyLocation.LocationType.Should().Be(locationType);
        familyLocation.Accuracy.Should().Be(accuracy);
        familyLocation.Source.Should().Be(source);
    }

    [Fact]
    public void FamilyLocation_ShouldApplyDefaultValuesForAccuracyAndSource()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var name = "Default Location";
                    var locationType = LocationType.Grave;
        // Act
        var familyLocation = new FamilyLocation
        {
            FamilyId = familyId,
            Name = name,
            LocationType = locationType
        };

        // Assert
        familyLocation.Accuracy.Should().Be(LocationAccuracy.Estimated);
        familyLocation.Source.Should().Be(LocationSource.UserSelected);
    }


}
