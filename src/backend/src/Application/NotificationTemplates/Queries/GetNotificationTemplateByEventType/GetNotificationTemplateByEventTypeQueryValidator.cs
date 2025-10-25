namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateByEventType;

public class GetNotificationTemplateByEventTypeQueryValidator : AbstractValidator<GetNotificationTemplateByEventTypeQuery>
{
    public GetNotificationTemplateByEventTypeQueryValidator()
    {
        RuleFor(v => v.EventType)
            .IsInEnum()
            .WithMessage("Loại sự kiện không hợp lệ.");

        RuleFor(v => v.Channel)
            .IsInEnum()
            .WithMessage("Kênh thông báo không hợp lệ.");
    }
}
