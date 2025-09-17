using backend.Domain.Entities;
using MongoDB.Driver;

namespace backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IMongoCollection<Family> Families { get; }
    IMongoCollection<Member> Members { get; }
    IMongoCollection<Relationship> Relationships { get; }
}
