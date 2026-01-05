using backend.Domain.Enums; // Add this

namespace backend.Application.LocationLinks.Queries;

public record LocationLinkDto
{
    public Guid Id { get; init; }
    public string RefId { get; init; } = null!;
    public RefType RefType { get; init; }
    public string Description { get; init; } = null!;
    public Guid LocationId { get; init; }
    public LocationLinkType LinkType { get; init; }

    // Navigation properties can be mapped if needed, e.g., Location details
    public LocationDto? Location { get; init; } // Assuming LocationDto exists or will be created
}

public record LocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? Address { get; init; }
    public string LocationType { get; init; } = null!;
    public string Accuracy { get; init; } = null!;
    public string Source { get; init; } = null!;
}
