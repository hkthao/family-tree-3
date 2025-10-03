using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdSpecification : BaseSpecification<Family>
{
    public FamilyByIdSpecification(Guid id)
    {
        AddCriteria(family => family.Id == id);
    }
}