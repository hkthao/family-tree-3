
namespace backend.Domain.Entities;

public class Family : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Logo { get; set; }
    public string? History { get; set; }
    public IList<Member> Members { get; private set; } = new List<Member>();
}
