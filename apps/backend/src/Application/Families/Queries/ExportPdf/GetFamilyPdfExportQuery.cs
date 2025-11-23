using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.ExportPdf;

public record GetFamilyPdfExportQuery(Guid FamilyId) : IRequest<Result<ExportedPdfFile>>;

public record ExportedPdfFile(byte[] Content, string FileName, string ContentType);