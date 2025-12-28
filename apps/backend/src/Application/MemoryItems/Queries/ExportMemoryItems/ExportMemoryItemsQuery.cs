using backend.Application.Common.Models;

namespace backend.Application.MemoryItems.Queries.ExportMemoryItems;

public record ExportMemoryItemsQuery(Guid FamilyId) : IRequest<Result<string>>;
