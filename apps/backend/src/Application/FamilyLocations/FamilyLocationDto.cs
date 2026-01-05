using backend.Application.Common.Mappings;
using backend.Application.Locations;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations;

public class FamilyLocationDto : IMapFrom<FamilyLocation>
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid LocationId { get; set; }
    public LocationDto Location { get; set; } = null!;
    public bool IsPrivate { get; set; } = false; // Flag to indicate if some properties were hidden due to privacy
}


