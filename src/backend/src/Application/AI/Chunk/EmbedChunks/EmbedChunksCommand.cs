using backend.Domain.Entities;
using backend.Application.Common.Models;

namespace backend.Application.AI.Chunk.EmbedChunks;

public class EmbedChunksCommand : IRequest<Result>
{
    public List<TextChunk> Chunks { get; set; } = [];
}
