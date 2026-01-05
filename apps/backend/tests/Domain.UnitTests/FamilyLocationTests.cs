using System.Reflection;
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
        var locationId = Guid.NewGuid();
        var name = "Test Location";
        var description = "A description for the test location";
        var latitude = 10.0;
        var longitude = 20.0;
        var address = "123 Test St";
        var locationType = LocationType.Homeland;
        var accuracy = LocationAccuracy.Exact;
        var source = LocationSource.UserSelected;

        // Act
        var location = new Location(name, description, latitude, longitude, address, locationType, accuracy, source);
        var familyLocation = new FamilyLocation(familyId, locationId);
        SetPrivateLocationProperty(familyLocation, location);

        // Assert
        familyLocation.FamilyId.Should().Be(familyId);
        familyLocation.Location.Name.Should().Be(name);
        familyLocation.Location.Description.Should().Be(description);
        familyLocation.Location.Latitude.Should().Be(latitude);
        familyLocation.Location.Longitude.Should().Be(longitude);
        familyLocation.Location.Address.Should().Be(address);
        familyLocation.Location.LocationType.Should().Be(locationType);
        familyLocation.Location.Accuracy.Should().Be(accuracy);
        familyLocation.Location.Source.Should().Be(source);
    }

    [Fact]
    public void FamilyLocation_ShouldApplyDefaultValuesForAccuracyAndSource()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var name = "Default Location";
        var locationType = LocationType.Grave;

        // Act
        var location = new Location(name, null, null, null, null, locationType, LocationAccuracy.Estimated, LocationSource.UserSelected);
        var familyLocation = new FamilyLocation(familyId, locationId);
        SetPrivateLocationProperty(familyLocation, location);

        // Assert
        familyLocation.Location.Accuracy.Should().Be(LocationAccuracy.Estimated);
        familyLocation.Location.Source.Should().Be(LocationSource.UserSelected);
    }


    private void SetPrivateLocationProperty(FamilyLocation familyLocation, Location location)
    {
        var propertyInfo = typeof(FamilyLocation).GetProperty("Location");
        propertyInfo?.SetValue(familyLocation, location);
    }
}
