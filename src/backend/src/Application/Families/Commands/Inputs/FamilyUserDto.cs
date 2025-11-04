using backend.Domain.Enums;

namespace backend.Application.Families.Commands.Inputs;

public record FamilyUserDto
{
    public Guid UserId { get; set; }
    public FamilyRole Role { get; set; } // Corresponds to FamilyRole enum
}