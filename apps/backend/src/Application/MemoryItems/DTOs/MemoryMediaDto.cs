using backend.Domain.Enums;

namespace backend.Application.MemoryItems.DTOs;

public class MemoryMediaDto
{
    public Guid Id { get; set; }
    public MemoryMediaType MediaType { get; set; }
    public string Url { get; set; } = string.Empty;
}
