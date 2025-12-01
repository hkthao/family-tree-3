namespace backend.Application.PdfTemplates.Dtos;

public class PdfTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string CssContent { get; set; } = string.Empty;
    public string Placeholders { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
}
