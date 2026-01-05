using backend.Domain.Enums;
using backend.Domain.Entities;
using backend.Application.Common.Mappings;

namespace backend.Application.Locations;

public class LocationDto : IMapFrom<Location>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy Accuracy { get; set; } = LocationAccuracy.Estimated;
    public LocationSource Source { get; set; } = LocationSource.UserSelected;
}
