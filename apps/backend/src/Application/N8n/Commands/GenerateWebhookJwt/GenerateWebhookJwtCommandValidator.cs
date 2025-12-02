namespace backend.Application.N8n.Commands.GenerateWebhookJwt;

public class GenerateWebhookJwtCommandValidator : AbstractValidator<GenerateWebhookJwtCommand>
{
    public GenerateWebhookJwtCommandValidator()
    {
        RuleFor(v => v.Subject)
            .NotEmpty().WithMessage("Subject là bắt buộc.");

        RuleFor(v => v.ExpiresInMinutes)
            .GreaterThan(0).WithMessage("Thời gian hết hạn phải lớn hơn 0 phút.")
            .LessThanOrEqualTo(1440).WithMessage("Thời gian hết hạn không được vượt quá 1440 phút (24 giờ)."); // Giới hạn 24 giờ
    }
}
