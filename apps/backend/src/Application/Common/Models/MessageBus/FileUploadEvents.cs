using backend.Application.Common.Models;

namespace backend.Application.Common.Models.MessageBus;

public class FileUploadRequestedEvent : IntegrationEvent
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string TempLocalPath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? Folder { get; set; }
    public Guid UploadedBy { get; set; }
    public long FileSize { get; set; }
    public Guid? FamilyId { get; set; } // Optional, for family-specific uploads
}

public class FileUploadCompletedEvent : IntegrationEvent
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = null!; // Keep original for context
    public string FinalFileUrl { get; set; } = null!;
    public string? DeleteHash { get; set; } // For services like Imgur/Cloudinary
    public Guid UploadedBy { get; set; }
    public Guid? FamilyId { get; set; } // Optional
    public string? Error { get; set; } // Added for error reporting
}

public class FileDeletionRequestedEvent : IntegrationEvent
{
    public Guid FileId { get; set; }
    public string FilePath { get; set; } = null!; // The URL or ID of the file to delete
    public string? DeleteHash { get; set; } // If available, for Cloudinary/Imgur specific deletion
    public Guid RequestedBy { get; set; }
    public Guid? FamilyId { get; set; } // Optional
}

public class FileDeletionCompletedEvent : IntegrationEvent
{
    public Guid FileId { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public Guid? FamilyId { get; set; } // Optional
}