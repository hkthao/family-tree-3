using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;

namespace backend.Application.PdfTemplates.Queries.GetPdfTemplate;

public record GetPdfTemplateQuery(Guid Id, Guid FamilyId) : IRequest<Result<PdfTemplateDto>>;
