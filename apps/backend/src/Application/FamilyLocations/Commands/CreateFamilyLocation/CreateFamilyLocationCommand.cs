using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Commands.CreateFamilyLocation;

public record CreateFamilyLocationCommand : IRequest<Result<Guid>>
{
    public Guid FamilyId { get; set; }
    public string LocationName { get; set; } = null!;
    public string? LocationDescription { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public string? LocationAddress { get; set; }
    public LocationType LocationType { get; set; }
    public LocationAccuracy LocationAccuracy { get; set; }
    public LocationSource LocationSource { get; set; }
}
