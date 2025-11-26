namespace backend.Application.PrivacyConfigurations.Queries;

public class PrivacyConfigurationDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public List<string> PublicMemberProperties { get; set; } = new List<string>();
}
