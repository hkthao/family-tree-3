namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;

public class GetNotificationTemplateByIdQueryValidator : AbstractValidator<GetNotificationTemplateByIdQuery>
{
    public GetNotificationTemplateByIdQueryValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID không được để trống.");
    }
}
