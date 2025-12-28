namespace backend.Domain.Entities;

public class PrivacyConfiguration : BaseAuditableEntity
{
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    // Stores a comma-separated string of public member property names
    public string PublicMemberProperties { get; private set; } = "";

    // Stores a comma-separated string of public event property names
    public string PublicEventProperties { get; private set; } = "";

    public PrivacyConfiguration(Guid familyId)
    {
        FamilyId = familyId;
    }

    public void UpdatePublicMemberProperties(List<string> publicMemberProperties)
    {
        PublicMemberProperties = string.Join(",", publicMemberProperties.OrderBy(p => p));
    }

    public List<string> GetPublicMemberPropertiesList()
    {
        return PublicMemberProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void UpdatePublicEventProperties(List<string> publicEventProperties)
    {
        PublicEventProperties = string.Join(",", publicEventProperties.OrderBy(p => p));
    }

    public List<string> GetPublicEventPropertiesList()
    {
        return PublicEventProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
