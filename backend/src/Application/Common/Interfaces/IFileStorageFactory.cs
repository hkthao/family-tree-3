using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces
{
    public interface IFileStorageFactory
    {
        IFileStorage CreateFileStorage(StorageProvider provider);
    }
}
