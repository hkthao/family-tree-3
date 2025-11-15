using backend.Application.Common.Models;

namespace backend.Application.Families.ExportImport;

public record ImportFamilyCommand(FamilyExportDto FamilyData) : IRequest<Result<Guid>>;
