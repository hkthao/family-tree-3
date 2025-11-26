using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;

namespace backend.Application.Memories.Queries.GetMemoryDetail;

public record GetMemoryDetailQuery(Guid Id) : IRequest<Result<MemoryDto>>;
