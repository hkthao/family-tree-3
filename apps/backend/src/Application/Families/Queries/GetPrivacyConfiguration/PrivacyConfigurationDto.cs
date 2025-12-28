namespace backend.Application.Families.Queries.GetPrivacyConfiguration;

public class PrivacyConfigurationDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public List<string> PublicMemberProperties { get; set; } = new List<string>();
    public List<string> PublicEventProperties { get; set; } = new List<string>();
    public List<string> PublicFamilyProperties { get; set; } = new List<string>();
    public List<string> PublicFamilyLocationProperties { get; set; } = new List<string>();
    public List<string> PublicMemoryItemProperties { get; set; } = new List<string>();
    public List<string> PublicMemberFaceProperties { get; set; } = new List<string>();
    public List<string> PublicFoundFaceProperties { get; set; } = new List<string>();
}
