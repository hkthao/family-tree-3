using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Files
{
    public class FileStorageFactory : IFileStorageFactory
    {
        private readonly IOptions<StorageSettings> _storageSettingsOptions;

        public FileStorageFactory(IOptions<StorageSettings> storageSettingsOptions)
        {
            _storageSettingsOptions = storageSettingsOptions;
        }

        public IFileStorage CreateFileStorage(StorageProvider provider)
        {
            var storageSettings = _storageSettingsOptions.Value;

            return provider switch
            {
                StorageProvider.Local => new LocalFileStorage(storageSettings),
                StorageProvider.Cloudinary => new CloudinaryFileStorage(storageSettings),
                StorageProvider.S3 => new S3FileStorage(storageSettings),
                _ => throw new InvalidOperationException($"No file storage provider configured for: {provider}")
            };
        }
    }
}
