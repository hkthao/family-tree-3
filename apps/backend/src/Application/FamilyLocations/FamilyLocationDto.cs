using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations;

public class FamilyLocationDto : IMapFrom<FamilyLocation>
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy Accuracy { get; set; }
    public LocationSource Source { get; set; }
}

