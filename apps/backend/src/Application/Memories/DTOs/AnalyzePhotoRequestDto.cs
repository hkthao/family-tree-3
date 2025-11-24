using Microsoft.AspNetCore.Http; // For IFormFile

namespace backend.Application.Memories.DTOs;

public class AnalyzePhotoRequestDto
{
    public IFormFile File { get; set; } = default!;
    public Guid? MemberId { get; set; }
}
