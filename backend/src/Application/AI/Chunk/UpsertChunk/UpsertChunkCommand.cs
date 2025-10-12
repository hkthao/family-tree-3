using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.AI.Chunk.UpsertChunk;

public record UpsertChunkCommand(TextChunk Chunk) : IRequest<Result>;
