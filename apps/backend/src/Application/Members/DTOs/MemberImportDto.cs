using backend.Domain.Enums;

namespace backend.Application.Members.DTOs;

public class MemberImportDto
{
    public Guid Id { get; set; } // Original ID, used for mapping relationships during import
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Nickname { get; set; }
    public string? Gender { get; set; } // Stored as string, will be parsed to enum if needed by entity
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Biography { get; set; }
    public bool IsDeceased { get; set; } = false;
    public bool IsRoot { get; set; } = false;
    public int? Order { get; set; }

    // Relationship IDs (original IDs from DTO)
    public Guid? FatherId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? HusbandId { get; set; }
    public Guid? WifeId { get; set; }
}
