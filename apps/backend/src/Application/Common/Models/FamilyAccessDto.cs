using backend.Domain.Enums;

namespace backend.Application.Common.Models;

public class FamilyAccessDto
{
    /// <summary>
    /// ID của gia đình.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Vai trò của người dùng trong gia đình.
    /// </summary>
    public FamilyRole Role { get; set; }
}
