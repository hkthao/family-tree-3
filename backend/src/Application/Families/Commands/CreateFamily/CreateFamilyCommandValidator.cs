namespace backend.Application.Families.Commands.CreateFamily
{
    public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
    {
        public CreateFamilyCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Tên dòng họ không được để trống.")
                .MaximumLength(200).WithMessage("Tên dòng họ không được vượt quá 200 ký tự.");
        }
    }
}
