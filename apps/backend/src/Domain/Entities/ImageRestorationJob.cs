using backend.Domain.Common;
using backend.Domain.Enums;
using System;

namespace backend.Domain.Entities;

public class ImageRestorationJob : BaseAuditableEntity
{
    public string JobId { get; set; } = string.Empty; // ID from the external service, now string
    public string OriginalImageUrl { get; set; } = string.Empty; // Renamed from ImageUrl
    public string UserId { get; set; } = string.Empty; // ID of the user who initiated the job
    public string FamilyId { get; set; } = string.Empty; // Family associated with the job
    public RestorationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestoredImageUrl { get; set; }
}
