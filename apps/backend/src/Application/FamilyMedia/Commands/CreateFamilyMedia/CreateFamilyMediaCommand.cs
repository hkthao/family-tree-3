using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace backend.Application.FamilyMedia.Commands.CreateFamilyMedia;

public record CreateFamilyMediaCommand : IRequest<Result<Guid>>
{
    public Guid FamilyId { get; init; }
    public IFormFile File { get; init; } = default!; // The uploaded file
    public MediaType? MediaType { get; init; }
    public string? Description { get; init; }
    public string? Folder { get; init; } // Folder within storage (e.g., "photos", "videos")
}
