using backend.Domain.Enums;

namespace backend.Application.Families.Dtos;

public class FamilyUserDto
{
    public Guid FamilyId { get; set; }
    public Guid UserId { get; set; }
    public FamilyRole Role { get; set; }
}
