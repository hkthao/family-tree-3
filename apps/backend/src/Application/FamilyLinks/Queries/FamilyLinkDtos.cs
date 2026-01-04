using backend.Application.Common.Dtos;

namespace backend.Application.FamilyLinks.Queries;
public class FamilyLinkDto : BaseAuditableDto
{
    public Guid Id { get; set; }
    public Guid Family1Id { get; set; }
    public string Family1Name { get; set; } = null!;
    public Guid Family2Id { get; set; }
    public string Family2Name { get; set; } = null!;
    public DateTime LinkDate { get; set; }
}
