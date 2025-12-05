using backend.Application.Common.Dtos;
using backend.Domain.Enums;

namespace backend.Application.FamilyLinks.Queries;

public class FamilyLinkRequestDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public Guid RequestingFamilyId { get; set; }
    public string RequestingFamilyName { get; set; } = null!;
    public Guid TargetFamilyId { get; set; }
    public string TargetFamilyName { get; set; } = null!;
    public LinkStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ResponseDate { get; set; }
}

public class FamilyLinkDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public Guid Family1Id { get; set; }
    public string Family1Name { get; set; } = null!;
    public Guid Family2Id { get; set; }
    public string Family2Name { get; set; } = null!;
    public DateTime LinkDate { get; set; }
}
