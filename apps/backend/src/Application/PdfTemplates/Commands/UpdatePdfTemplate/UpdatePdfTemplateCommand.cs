using backend.Application.Common.Models;

namespace backend.Application.PdfTemplates.Commands.UpdatePdfTemplate;

public record UpdatePdfTemplateCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;
    public string CssContent { get; init; } = string.Empty;
    public string Placeholders { get; init; } = string.Empty;
    public Guid FamilyId { get; init; } // FamilyId is required for authorization
}
