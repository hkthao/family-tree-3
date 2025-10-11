using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Files
{
    public class FileStorageFactory : IFileStorageFactory
    {
        private readonly IOptions<StorageSettings> _storageSettingsOptions;
        private readonly IWebHostEnvironment _env;

        public FileStorageFactory(IOptions<StorageSettings> storageSettingsOptions, IWebHostEnvironment env)
        {
            _storageSettingsOptions = storageSettingsOptions;
            _env = env;
        }

        public IFileStorage CreateFileStorage(StorageProvider provider)
        {
            var storageSettings = _storageSettingsOptions.Value;

            return provider switch
            {
                StorageProvider.Local => new LocalFileStorage(storageSettings, _env),
                StorageProvider.Cloudinary => new CloudinaryFileStorage(storageSettings),
                StorageProvider.S3 => new S3FileStorage(storageSettings),
                _ => throw new InvalidOperationException($"No file storage provider configured for: {provider}")
            };
        }
    }
}
