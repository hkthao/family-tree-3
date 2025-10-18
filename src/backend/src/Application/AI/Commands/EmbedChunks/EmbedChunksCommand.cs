using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.AI.Commands.EmbedChunks;

public class EmbedChunksCommand : IRequest<Result>
{
    public List<TextChunk> Chunks { get; set; } = [];
    public string ProviderName { get; set; } = null!;
}
