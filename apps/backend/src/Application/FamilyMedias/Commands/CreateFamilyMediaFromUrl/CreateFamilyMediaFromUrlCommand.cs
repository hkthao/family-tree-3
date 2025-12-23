using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMediaFromUrl;

public record CreateFamilyMediaFromUrlCommand : IRequest<Result<FamilyMediaDto>>
{
    public Guid FamilyId { get; init; }
    public string Url { get; init; } = default!; // The URL of the file
    public string FileName { get; init; } = default!; // The name of the file (for display)
    public MediaType? MediaType { get; init; }
    public string? Description { get; init; }
    public string? Folder { get; init; } // Folder within storage (e.g., "photos", "videos")
}
