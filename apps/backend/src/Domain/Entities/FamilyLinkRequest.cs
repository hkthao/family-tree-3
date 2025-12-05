using backend.Domain.Enums;
using System;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một yêu cầu liên kết giữa hai gia đình.
/// </summary>
public class FamilyLinkRequest : BaseAuditableEntity
{
    public Guid RequestingFamilyId { get; private set; }
    public Family RequestingFamily { get; private set; } = null!; // Navigation property

    public Guid TargetFamilyId { get; private set; }
    public Family TargetFamily { get; private set; } = null!; // Navigation property

    public LinkStatus Status { get; private set; } = LinkStatus.Pending;
    public DateTime RequestDate { get; private set; }
    public DateTime? ResponseDate { get; private set; } // When the request was approved or rejected

    // Private constructor for EF Core and internal use
    private FamilyLinkRequest() { }

    public FamilyLinkRequest(Guid requestingFamilyId, Guid targetFamilyId)
    {
        RequestingFamilyId = requestingFamilyId;
        TargetFamilyId = targetFamilyId;
        RequestDate = DateTime.UtcNow;
        Status = LinkStatus.Pending;
    }

    public void UpdateStatus(LinkStatus newStatus)
    {
        // Add any validation logic here if needed, e.g.,
        // if (Status == LinkStatus.Approved && newStatus == LinkStatus.Rejected) { ... }
        Status = newStatus;
        ResponseDate = DateTime.UtcNow; // Update response date on any status change
    }

    public void Approve()
    {
        if (Status != LinkStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be approved.");
        }
        Status = LinkStatus.Approved;
        ResponseDate = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != LinkStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be rejected.");
        }
        Status = LinkStatus.Rejected;
        ResponseDate = DateTime.UtcNow;
    }

    public void MarkAsPending()
    {
        Status = LinkStatus.Pending;
        ResponseDate = null;
    }
}
