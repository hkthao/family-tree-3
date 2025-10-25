namespace backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;

public class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
{
    public UpdateNotificationTemplateCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID không được để trống.");

        RuleFor(v => v.EventType)
            .IsInEnum()
            .WithMessage("Loại sự kiện không hợp lệ.");

        RuleFor(v => v.Channel)
            .IsInEnum()
            .WithMessage("Kênh thông báo không hợp lệ.");

        RuleFor(v => v.Subject)
            .NotEmpty().WithMessage("Chủ đề không được để trống.")
            .MaximumLength(250).WithMessage("Chủ đề không được vượt quá 250 ký tự.");

        RuleFor(v => v.Body)
            .NotEmpty().WithMessage("Nội dung không được để trống.");

        RuleFor(v => v.Format)
            .IsInEnum()
            .WithMessage("Định dạng không hợp lệ.");

        RuleFor(v => v.LanguageCode)
            .NotEmpty().WithMessage("Mã ngôn ngữ không được để trống.")
            .MaximumLength(10).WithMessage("Mã ngôn ngữ không được vượt quá 10 ký tự.");
    }
}
