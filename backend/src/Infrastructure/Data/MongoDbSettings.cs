namespace backend.Infrastructure.Data;

public class AppMongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}
