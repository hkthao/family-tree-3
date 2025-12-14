using backend.Application.FamilyMedias.DTOs;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMedia;

public record CreateFamilyMediaCommand : IRequest<Result<FamilyMediaDto>>
{
    public Guid FamilyId { get; init; }
    public byte[] File { get; init; } = default!; // The uploaded file as byte array
    public string FileName { get; init; } = default!; // The name of the file
    public MediaType? MediaType { get; init; }
    public string? Description { get; init; }
    public string? Folder { get; init; } // Folder within storage (e.g., "photos", "videos")
}
