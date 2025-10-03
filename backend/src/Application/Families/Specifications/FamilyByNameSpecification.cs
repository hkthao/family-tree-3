using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByNameSpecification : BaseSpecification<Family>
{
    public FamilyByNameSpecification(string name)
    {
        AddCriteria(family => family.Name.Contains(name));
    }
}