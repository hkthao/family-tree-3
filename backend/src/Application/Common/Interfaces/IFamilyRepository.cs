using backend.Domain.Entities;
using backend.Domain.Common.Interfaces;

namespace backend.Application.Common.Interfaces;

public interface IFamilyRepository : IRepository<Family>
{
    // Add any family-specific repository methods here if needed
}