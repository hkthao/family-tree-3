namespace backend.Application.Relationships.Commands.CreateRelationships;

public class CreateRelationshipsCommandValidator : AbstractValidator<CreateRelationshipsCommand>
{
    public CreateRelationshipsCommandValidator()
    {
        RuleFor(x => x.Relationships)
            .NotEmpty().WithMessage("Danh sách mối quan hệ không được để trống.");

        RuleForEach(x => x.Relationships).ChildRules(relationship =>
        {
            relationship.RuleFor(r => r.SourceMemberId)
                .NotEmpty().WithMessage("ID thành viên nguồn không được để trống.");

            relationship.RuleFor(r => r.TargetMemberId)
                .NotEmpty().WithMessage("ID thành viên đích không được để trống.");

            relationship.RuleFor(r => r.Type)
                .IsInEnum().WithMessage("Loại mối quan hệ không hợp lệ.");

            relationship.RuleFor(r => r)
                .Must(r => r.SourceMemberId != r.TargetMemberId)
                .WithMessage("Thành viên nguồn và thành viên đích không được giống nhau.");
        });
    }
}
