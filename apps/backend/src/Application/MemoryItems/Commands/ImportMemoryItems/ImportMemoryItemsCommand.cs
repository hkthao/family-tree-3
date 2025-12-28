using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;

namespace backend.Application.MemoryItems.Commands.ImportMemoryItems;

public record ImportMemoryItemsCommand(Guid FamilyId, List<MemoryItemDto> MemoryItems) : IRequest<Result<Unit>>;
