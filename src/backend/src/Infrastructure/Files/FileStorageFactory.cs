using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.Files;

public class FileStorageFactory : IFileStorageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FileStorageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileStorage CreateFileStorage(StorageProvider provider)
    {
        return provider switch
        {
            StorageProvider.Local => _serviceProvider.GetRequiredService<LocalFileStorage>(),
            StorageProvider.Cloudinary => _serviceProvider.GetRequiredService<CloudinaryFileStorage>(),
            StorageProvider.S3 => _serviceProvider.GetRequiredService<S3FileStorage>(),
            _ => throw new InvalidOperationException($"No file storage provider configured for: {provider}")
        };
    }
}
