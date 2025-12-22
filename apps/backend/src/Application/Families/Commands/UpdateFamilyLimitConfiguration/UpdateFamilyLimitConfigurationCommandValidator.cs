namespace backend.Application.Families.Commands.UpdateFamilyLimitConfiguration;

/// <summary>
/// Trình xác thực cho UpdateFamilyLimitConfigurationCommand.
/// </summary>
public class UpdateFamilyLimitConfigurationCommandValidator : AbstractValidator<UpdateFamilyLimitConfigurationCommand>
{
    public UpdateFamilyLimitConfigurationCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("ID gia đình là bắt buộc.");

        RuleFor(v => v.MaxMembers)
            .GreaterThan(0).WithMessage("Số lượng thành viên tối đa phải lớn hơn 0.");

        RuleFor(v => v.MaxStorageMb)
            .GreaterThan(0).WithMessage("Dung lượng lưu trữ tối đa phải lớn hơn 0.");

        RuleFor(v => v.AiChatMonthlyLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Giới hạn trò chuyện AI hàng tháng không thể âm.");
    }
}
