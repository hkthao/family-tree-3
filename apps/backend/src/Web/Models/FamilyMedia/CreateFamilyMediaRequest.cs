namespace backend.Web.Models.FamilyMedia;

public class CreateFamilyMediaRequest
{
    public Guid FamilyId { get; set; }
    public IFormFile File { get; set; } = default!; // The uploaded file as IFormFile
    public string? Description { get; set; }
    public string? Folder { get; set; } // Folder within storage (e.g., "photos", "videos")
}
