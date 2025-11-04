using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.AI.Chunk.EmbedChunks;

/// <summary>
/// Đại diện cho một lệnh để nhúng (embed) một danh sách các đoạn văn bản (TextChunk).
/// </summary>
public class EmbedChunksCommand : IRequest<Result>
{
    public EmbedChunksCommand()
    {
    }

    public EmbedChunksCommand(List<TextChunk> chunks)
    {
        Chunks = chunks;
    }

    /// <summary>
    /// Danh sách các đoạn văn bản cần được nhúng.
    /// </summary>
    public List<TextChunk> Chunks { get; set; } = [];
}
