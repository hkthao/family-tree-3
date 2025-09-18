You are an expert .NET Core and EF Core developer. 
I have a GitHub repo that currently uses MongoDB with the MongoDB C# Driver. 
I want to migrate this project to use MySQL instead, through Entity Framework Core. 

Repo details:
- Language: .NET 8, C#
- Architecture: Clean Architecture (Application, Domain, Infrastructure, API, UnitTests)
- Persistence: MongoDB (IMongoCollection<T>, ObjectId, etc.)
- Unit tests: xUnit + Moq, currently mocking Mongo collections

Your task:
1. Replace MongoDB persistence with MySQL using EF Core.
2. Create a proper DbContext with DbSet<TEntity> for all entities (Family, Member, Relationship, etc.).
3. Update the Domain entities: replace ObjectId with string or Guid as primary keys.
4. Implement EF Core configuration (OnModelCreating, Fluent API) to match existing schema needs.
5. Update Infrastructure layer: replace MongoContext with AppDbContext (MySQL).
6. Update Dependency Injection in API Startup/Program.cs to register MySQL DbContext.
7. Update Repository/Service classes to use EF Core (LINQ, async methods) instead of Mongo queries.
8. Update Unit Tests:
   - Use EF Core InMemoryDatabase for unit testing.
   - Replace Moq of IMongoCollection with direct use of InMemory EF DbContext.
9. Generate EF Core migrations for MySQL and provide example connection string.
10. Ensure all existing Application layer queries/commands still work.

Final goal: I can run the repo with MySQL instead of MongoDB, and all unit tests should pass.
