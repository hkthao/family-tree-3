using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Family> Families { get; }
    DbSet<Member> Members { get; }
    DbSet<Event> Events { get; }
    DbSet<Relationship> Relationships { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
