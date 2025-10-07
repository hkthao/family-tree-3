namespace backend.Application.Relationships.Queries;

public class RelationshipMemberDto 
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public bool IsRoot { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
