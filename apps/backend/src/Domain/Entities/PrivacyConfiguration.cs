namespace backend.Domain.Entities;

public class PrivacyConfiguration : BaseAuditableEntity
{
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    // Stores a comma-separated string of public member property names
    public string PublicMemberProperties { get; private set; } = "";

    // Stores a comma-separated string of public event property names
    public string PublicEventProperties { get; private set; } = "";

    // Stores a comma-separated string of public family property names
    public string PublicFamilyProperties { get; private set; } = "";

    // Stores a comma-separated string of public family location property names
    public string PublicFamilyLocationProperties { get; private set; } = "";

    // Stores a comma-separated string of public memory item property names
    public string PublicMemoryItemProperties { get; private set; } = "";

    // Stores a comma-separated string of public member face property names
    public string PublicMemberFaceProperties { get; private set; } = "";

    // Stores a comma-separated string of public found face property names
    public string PublicFoundFaceProperties { get; private set; } = "";

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

    public void UpdatePublicFamilyProperties(List<string> publicFamilyProperties)
    {
        PublicFamilyProperties = string.Join(",", publicFamilyProperties.OrderBy(p => p));
    }

    public List<string> GetPublicFamilyPropertiesList()
    {
        return PublicFamilyProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void UpdatePublicFamilyLocationProperties(List<string> publicFamilyLocationProperties)
    {
        PublicFamilyLocationProperties = string.Join(",", publicFamilyLocationProperties.OrderBy(p => p));
    }

    public List<string> GetPublicFamilyLocationPropertiesList()
    {
        return PublicFamilyLocationProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void UpdatePublicMemoryItemProperties(List<string> publicMemoryItemProperties)
    {
        PublicMemoryItemProperties = string.Join(",", publicMemoryItemProperties.OrderBy(p => p));
    }

    public List<string> GetPublicMemoryItemPropertiesList()
    {
        return PublicMemoryItemProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void UpdatePublicMemberFaceProperties(List<string> publicMemberFaceProperties)
    {
        PublicMemberFaceProperties = string.Join(",", publicMemberFaceProperties.OrderBy(p => p));
    }

    public List<string> GetPublicMemberFacePropertiesList()
    {
        return PublicMemberFaceProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void UpdatePublicFoundFaceProperties(List<string> publicFoundFaceProperties)
    {
        PublicFoundFaceProperties = string.Join(",", publicFoundFaceProperties.OrderBy(p => p));
    }

    public List<string> GetPublicFoundFacePropertiesList()
    {
        return PublicFoundFaceProperties.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}