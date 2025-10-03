namespace backend.Application.Members.Commands;

public record UpdateMemberCommand : IRequest
{
    public Guid Id { get; init; }
    public string LastName { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string? Nickname { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public DateTime? DateOfDeath { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? PlaceOfDeath { get; init; }
    public string? Gender { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Occupation { get; init; }
    public string? Biography { get; init; }
    public Guid FamilyId { get; init; }
    public Guid? FatherId { get; init; }
    public Guid? MotherId { get; init; }
    public Guid? SpouseId { get; init; }
}