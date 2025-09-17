The job failed because a MongoDB connection string was missing or set to null when calling AddMongoDbStores in your ASP.NET Core project's startup/configuration. The error is:

```
System.ArgumentNullException: Value cannot be null. (Parameter 'connectionString')
at Microsoft.Extensions.DependencyInjection.MongoDbIdentityBuilderExtensions.AddMongoDbStores[TUser,TRole,TKey](IdentityBuilder builder, String connectionString, String databaseName)
```

## Solution

### 1. Set the MongoDB Connection String

Ensure the connection string for MongoDB is provided correctly in your configuration (commonly in `appsettings.json`, environment variables, or CI secrets).

Example for `appsettings.json`:
```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://youruser:yourpassword@yourhost:27017/yourdb",
    "DatabaseName": "yourdb"
  }
}
```

### 2. Pass the Connection String to AddMongoDbStores

In your startup/configuration code (e.g., `Startup.cs` or wherever Identity is configured):

```csharp
var connectionString = Configuration.GetSection("MongoDb:ConnectionString").Value;
var databaseName = Configuration.GetSection("MongoDb:DatabaseName").Value;

services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(connectionString, databaseName);
```

### 3. For CI/CD (GitHub Actions)

If you use environment variables or secrets in your workflow, ensure they are set and mapped in your workflow YAML (e.g., `.github/workflows/ci.yml`):

```yaml
env:
  MONGODB_CONNECTIONSTRING: ${{ secrets.MONGODB_CONNECTIONSTRING }}
  MONGODB_DATABASENAME: ${{ secrets.MONGODB_DATABASENAME }}
```
And in code:
```csharp
var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTIONSTRING");
var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASENAME");
```

## Code Suggestion

- Add a null check and log error if connection string is not set.
- Ensure your config file or CI secrets include the MongoDB connection string.

## Next Steps

- Update your configuration and restart the build.
- If you need help identifying where to set the connection string in your repo, let me know and I can point to the relevant file.

[See failing code in context](https://github.com/hkthao/family-tree-3/blob/2f2fa4a950be9113eec052da473d1899161fa291/backend/src/Web/Web.csproj)