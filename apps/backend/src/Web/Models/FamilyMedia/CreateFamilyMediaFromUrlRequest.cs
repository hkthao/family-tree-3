using backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace backend.Web.Models.FamilyMedia;

public class CreateFamilyMediaFromUrlRequest
{
    [Required(ErrorMessage = "URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(2000, ErrorMessage = "URL cannot exceed 2000 characters.")]
    public string Url { get; set; } = string.Empty;

    [Required(ErrorMessage = "File name is required.")]
    [StringLength(250, ErrorMessage = "File name cannot exceed 250 characters.")]
    public string FileName { get; set; } = string.Empty;

    public MediaType? MediaType { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    public string? Folder { get; set; }
}
