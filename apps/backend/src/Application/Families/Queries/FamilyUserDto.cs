using backend.Domain.Enums;

namespace backend.Application.Families.Dtos;

public class FamilyUserDto
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public FamilyRole Role { get; set; }
}
