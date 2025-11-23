using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.ExportPdf;

public record GetFamilyPdfExportQuery(Guid FamilyId, string HtmlContent) : IRequest<Result<ExportedPdfFile>>;

public record ExportedPdfFile(byte[] Content, string FileName, string ContentType);