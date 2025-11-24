using backend.Application.Common.Models;

namespace backend.Application.Memories.Commands.DeleteMemory;

public record DeleteMemoryCommand(Guid Id) : IRequest<Result>;
