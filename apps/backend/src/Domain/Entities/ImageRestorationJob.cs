using System;
using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class ImageRestorationJob : BaseAuditableEntity
{
    public string JobId { get; set; } = string.Empty; // ID from the external service, now string
    public string OriginalImageUrl { get; set; } = string.Empty; // Renamed from ImageUrl
    public string UserId { get; set; } = string.Empty; // ID of the user who initiated the job
    public Guid FamilyId { get; set; } // Family associated with the job
    public Family Family { get; set; } = null!;
    public RestorationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestoredImageUrl { get; set; }

    /// <summary>
    /// Initializes the image restoration job with basic details.
    /// </summary>
    public void Initialize(string originalImageUrl, string userId, Guid familyId)
    {
        OriginalImageUrl = originalImageUrl;
        UserId = userId;
        FamilyId = familyId;
        Status = RestorationStatus.Processing;
    }

    /// <summary>
    /// Marks the image restoration job as failed with a given error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    public void MarkAsFailed(string errorMessage)
    {
        Status = RestorationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Marks the image restoration job as processing and sets the external job ID.
    /// </summary>
    /// <param name="jobId">The ID provided by the external image restoration service.</param>
    public void MarkAsProcessing(string jobId)
    {
        JobId = jobId;
        Status = RestorationStatus.Processing;
    }

    /// <summary>
    /// Marks the image restoration job as completed and sets the restored image URL.
    /// </summary>
    /// <param name="restoredImageUrl">The URL of the restored image.</param>
    public void MarkAsCompleted(string restoredImageUrl)
    {
        RestoredImageUrl = restoredImageUrl;
        Status = RestorationStatus.Completed;
    }
}
