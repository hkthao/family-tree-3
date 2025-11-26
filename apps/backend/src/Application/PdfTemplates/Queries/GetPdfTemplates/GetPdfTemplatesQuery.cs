using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;

namespace backend.Application.PdfTemplates.Queries.GetPdfTemplates;

public record GetPdfTemplatesQuery(Guid FamilyId) : IRequest<Result<List<PdfTemplateDto>>>;
