namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Trình xác thực cho GenerateFamilyDataCommand.
/// </summary>
public class GenerateFamilyDataCommandValidator : AbstractValidator<GenerateFamilyDataCommand>
{
    public GenerateFamilyDataCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId không được để trống.");

        RuleFor(v => v.ChatInput)
            .NotEmpty().WithMessage("ChatInput không được để trống.")
            .Must(BeWithinWordLimit).WithMessage("ChatInput không được vượt quá 200 từ.");
    }

    private bool BeWithinWordLimit(string chatInput)
    {
        if (string.IsNullOrWhiteSpace(chatInput))
        {
            return true;
        }
        var wordCount = chatInput.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return wordCount <= 200;
    }

}
