using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;
using MediatR;
using System;

namespace backend.Application.PdfTemplates.Commands.CreatePdfTemplate;

public record CreatePdfTemplateCommand : IRequest<Result<PdfTemplateDto>>
{
    public string Name { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;
    public string CssContent { get; init; } = string.Empty;
    public string Placeholders { get; init; } = string.Empty;
    public Guid FamilyId { get; init; }
}