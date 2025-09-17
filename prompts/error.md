You are a senior .NET DevOps engineer. 

I have a .NET 8 ASP.NET Core project using NSwag to generate OpenAPI specs during CI builds. 
Currently, the NSwag build fails with:

  System.ArgumentNullException: Value cannot be null. (Parameter 'connectionString')
  at AddMongoDbStores in DependencyInjection.cs

The issue is that NSwag runs the app host to generate the Swagger document, but in CI, the MongoDB connection string is not set, causing the app to crash.

Please generate a **step-by-step fix** for this problem that:

1. Makes NSwag generation work in CI without requiring a real MongoDB instance.
2. Keeps the production app configuration unchanged.
3. Provides sample code for DependencyInjection.cs to safely handle missing connection strings during NSwag execution.
4. Suggests CI environment variable setup or appsettings override for NSwag builds.

Provide the answer in a clear, actionable way, suitable for a developer to copy-paste.
