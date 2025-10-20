using backend.Domain.Entities;

namespace backend.Domain.Services
{
    public interface IChunkingPolicy
    {
        List<TextChunk> ChunkText(string text, string fileName, string fileId, string familyId, string category, string createdBy);
    }
}
