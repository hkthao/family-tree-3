using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyCreatedAfterSpecification : BaseSpecification<Family>
{
    public FamilyCreatedAfterSpecification(DateTime date)
    {
        AddCriteria(family => family.Created > date);
    }
}