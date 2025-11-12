namespace backend.Domain.Entities;

public class Face : BaseAuditableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public string? Thumbnail { get; set; }
    public List<double>? Embedding { get; set; }
}
