using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Infrastructure.Data;

public class ApplicationDbContext : IApplicationDbContext
{
    private readonly IMongoDatabase _database;

    public ApplicationDbContext(IOptions<AppMongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<TodoList> TodoLists => _database.GetCollection<TodoList>("TodoLists");
    public IMongoCollection<TodoItem> TodoItems => _database.GetCollection<TodoItem>("TodoItems");
    public IMongoCollection<Family> Families => _database.GetCollection<Family>("Families");
    public IMongoCollection<Member> Members => _database.GetCollection<Member>("Members");
    public IMongoCollection<Relationship> Relationships => _database.GetCollection<Relationship>("Relationships");
}


