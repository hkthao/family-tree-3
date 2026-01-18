namespace backend.Application.Knowledge.DTOs;

public class FamilyKnowledgeDto
{
    public Guid FamilyId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? GenealogyRecord { get; set; }
    public string? ProgenitorName { get; set; }
    public string? FamilyCovenant { get; set; }
    public string Visibility { get; set; } = "Private";
    public string ContentType { get; set; } = "Family";
}

public class MemberKnowledgeDto
{
    public Guid FamilyId { get; set; }
    public Guid MemberId { get; set; }
    public string FullName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Gender { get; set; }
    public string? Biography { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string ContentType { get; set; } = "Member";
}

public class EventKnowledgeDto
{
    public Guid FamilyId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? CalendarType { get; set; }
    public DateTime? SolarDate { get; set; }
    public string ContentType { get; set; } = "Event";
}

public class KnowledgeIndexRequest<T>
{
    public T Data { get; set; } = default!;
    public string Action { get; set; } = "index"; // "index" or "delete"
}
