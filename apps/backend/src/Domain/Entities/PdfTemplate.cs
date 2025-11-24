using backend.Domain.Common;
using System;

namespace backend.Domain.Entities;

public class PdfTemplate : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string CssContent { get; set; } = string.Empty;
    public string Placeholders { get; set; } = string.Empty; // Store as JSON string or similar
    public Guid FamilyId { get; set; } // Template can be associated with a specific family
    public Family Family { get; set; } = null!; // Navigation property

    // Private constructor for EF Core and factory methods
    private PdfTemplate() { }

    public static PdfTemplate Create(string name, string htmlContent, string cssContent, string placeholders, Guid familyId)
    {
        var template = new PdfTemplate
        {
            Name = name,
            HtmlContent = htmlContent,
            CssContent = cssContent,
            Placeholders = placeholders,
            FamilyId = familyId
        };
        // Add domain events if needed
        return template;
    }

    public void Update(string name, string htmlContent, string cssContent, string placeholders)
    {
        Name = name;
        HtmlContent = htmlContent;
        CssContent = cssContent;
        Placeholders = placeholders;
        // Add domain events if needed
    }
}