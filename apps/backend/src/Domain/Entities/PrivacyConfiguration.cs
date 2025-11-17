using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Domain.Entities;

public class PrivacyConfiguration : BaseAuditableEntity
{
    public Guid FamilyId { get; private set; }
    public Family Family { get; private set; } = null!;

    // Stores a comma-separated string of public member property names
    public string PublicMemberProperties { get; private set; } = "";

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
}
