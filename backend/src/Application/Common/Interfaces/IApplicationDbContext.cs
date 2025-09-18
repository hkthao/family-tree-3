using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<Member> Members { get; }
    DbSet<Relationship> Relationships { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}