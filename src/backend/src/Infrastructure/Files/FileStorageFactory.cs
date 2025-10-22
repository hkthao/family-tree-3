using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.Files;

public class FileStorageFactory(IServiceScopeFactory serviceScopeFactory) : IFileStorageFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public IFileStorage CreateFileStorage(StorageProvider provider)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider; // Get the service provider for this scope

        return provider switch
        {
            StorageProvider.Local => serviceProvider.GetRequiredService<LocalFileStorage>(),
            StorageProvider.Cloudinary => serviceProvider.GetRequiredService<CloudinaryFileStorage>(),
            StorageProvider.S3 => serviceProvider.GetRequiredService<S3FileStorage>(),
            _ => throw new InvalidOperationException($"No file storage provider configured for: {provider}")
        };
    }
}
