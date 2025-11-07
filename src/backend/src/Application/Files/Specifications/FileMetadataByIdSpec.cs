using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Files.Specifications;

public class FileMetadataByIdSpec : Specification<FileMetadata>, ISingleResultSpecification<FileMetadata>
{
    public FileMetadataByIdSpec(Guid fileId)
    {
        Query.Where(fm => fm.Id == fileId);
    }
}
