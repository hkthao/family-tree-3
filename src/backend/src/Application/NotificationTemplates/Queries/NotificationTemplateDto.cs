using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Queries;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public NotificationType EventType { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public TemplateFormat Format { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string? Placeholders { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}