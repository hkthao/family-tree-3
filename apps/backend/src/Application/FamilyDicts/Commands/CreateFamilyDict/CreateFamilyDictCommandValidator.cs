using FluentValidation;

namespace backend.Application.FamilyDicts.Commands.CreateFamilyDict;

public class CreateFamilyDictCommandValidator : AbstractValidator<CreateFamilyDictCommand>
{
    public CreateFamilyDictCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .MaximumLength(200).WithMessage("Tên không được vượt quá 200 ký tự.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự.");

        RuleFor(v => v.NamesByRegion.North)
            .NotEmpty().WithMessage("Tên miền Bắc không được để trống.");

        // Thêm các quy tắc xác thực khác nếu cần
    }
}
