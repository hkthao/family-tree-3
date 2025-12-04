using backend.Domain.Common;

namespace backend.Domain.Entities;

public class Prompt : BaseAuditableEntity
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Description { get; set; }
}
