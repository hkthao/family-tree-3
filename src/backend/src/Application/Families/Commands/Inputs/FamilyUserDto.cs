namespace backend.Application.Families.Commands.Inputs;

public record FamilyUserDto
{
    public Guid UserId { get; init; }
    public int Role { get; init; } // Corresponds to FamilyRole enum
}