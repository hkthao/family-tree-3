using backend.Application.Common.Models;
using backend.Application.ExportImport.Commands;

namespace backend.Application.ExportImport.Queries;

public record GetFamilyExportQuery(Guid FamilyId) : IRequest<Result<FamilyExportDto>>;
