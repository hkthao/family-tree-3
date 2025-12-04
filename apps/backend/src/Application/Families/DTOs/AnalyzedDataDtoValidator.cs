using backend.Application.Families.DTOs;
using FluentValidation;

namespace backend.Application.Families.DTOs;

public class AnalyzedDataDtoValidator : AbstractValidator<AnalyzedDataDto>
{
    public AnalyzedDataDtoValidator()
    {
        RuleForEach(x => x.Members)
            .SetValidator(new MemberDataDtoValidator());

        RuleForEach(x => x.Events)
            .SetValidator(new EventDataDtoValidator());
    }
}

public class MemberDataDtoValidator : AbstractValidator<MemberDataDto>
{
    public MemberDataDtoValidator()
    {
        RuleFor(m => m.FirstName)
            .NotEmpty().WithMessage("Tên của thành viên không được để trống.");

        RuleFor(m => m.LastName)
            .NotEmpty().WithMessage("Họ của thành viên không được để trống.");
    }
}


public class EventDataDtoValidator : AbstractValidator<EventDataDto>
{
    public EventDataDtoValidator()
    {
        RuleFor(e => e.Type)
            .NotEmpty().WithMessage("Loại sự kiện không được để trống.");

        RuleFor(e => e.Description)
            .NotEmpty().WithMessage("Mô tả sự kiện không được để trống.");
    }
}
