using backend.Application.Common.Models;
using backend.Application.Families.ExportImport;

namespace backend.Application.Families.Queries.ExportImport;

public record GetFamilyExportQuery(Guid FamilyId) : IRequest<Result<FamilyExportDto>>;
