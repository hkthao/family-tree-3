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
    public string? RequestMessage { get; private set; }
    public string? ResponseMessage { get; private set; }

    // Private constructor for EF Core and internal use
    private FamilyLinkRequest() { }

    public FamilyLinkRequest(Guid requestingFamilyId, Guid targetFamilyId, string? requestMessage)
    {
        RequestingFamilyId = requestingFamilyId;
        TargetFamilyId = targetFamilyId;
        RequestDate = DateTime.UtcNow;
        Status = LinkStatus.Pending;
        RequestMessage = requestMessage;
    }

    public void Approve(string? responseMessage)
    {
        if (Status != LinkStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be approved.");
        }
        Status = LinkStatus.Approved;
        ResponseDate = DateTime.UtcNow;
        ResponseMessage = responseMessage;
    }

    public void Reject(string? responseMessage)
    {
        if (Status != LinkStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be rejected.");
        }
        Status = LinkStatus.Rejected;
        ResponseDate = DateTime.UtcNow;
        ResponseMessage = responseMessage;
    }

    public void MarkAsPending()
    {
        Status = LinkStatus.Pending;
        ResponseDate = null;
        ResponseMessage = null; // Clear response message when marking as pending
    }
}
