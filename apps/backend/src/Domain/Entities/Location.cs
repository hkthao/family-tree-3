using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class Location : BaseAuditableEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? Address { get; private set; }
    public LocationType LocationType { get; private set; }
    public LocationAccuracy Accuracy { get; private set; }
    public LocationSource Source { get; private set; }

    // Private constructor for EF Core and other internal uses
    private Location()
    {
        Name = null!; // For EF Core
    }

    public Location(string name, string? description, double? latitude, double? longitude, string? address, LocationType locationType, LocationAccuracy accuracy, LocationSource source)
    {
        Name = name;
        Description = description;
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        LocationType = locationType;
        Accuracy = accuracy;
        Source = source;
    }

    public void Update(string name, string? description, double? latitude, double? longitude, string? address, LocationType locationType, LocationAccuracy accuracy, LocationSource source)
    {
        Name = name;
        Description = description;
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        LocationType = locationType;
        Accuracy = accuracy;
        Source = source;
    }
}

