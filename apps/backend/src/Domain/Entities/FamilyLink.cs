using System;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một liên kết đã thiết lập giữa hai gia đình.
/// </summary>
public class FamilyLink : BaseAuditableEntity
{
    public Guid Family1Id { get; private set; }
    public Family Family1 { get; private set; } = null!; // Navigation property

    public Guid Family2Id { get; private set; }
    public Family Family2 { get; private set; } = null!; // Navigation property

    public DateTime LinkDate { get; private set; }

    // Private constructor for EF Core and internal use
    private FamilyLink() { }

    public FamilyLink(Guid family1Id, Guid family2Id)
    {
        // Đảm bảo Family1Id luôn nhỏ hơn Family2Id để tránh trùng lặp liên kết
        // (e.g., (A,B) is the same as (B,A))
        if (family1Id.CompareTo(family2Id) < 0)
        {
            Family1Id = family1Id;
            Family2Id = family2Id;
        }
        else
        {
            Family1Id = family2Id;
            Family2Id = family1Id;
        }

        LinkDate = DateTime.UtcNow;
    }
}
