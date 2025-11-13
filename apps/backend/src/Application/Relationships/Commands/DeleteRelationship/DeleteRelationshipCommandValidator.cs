namespace backend.Application.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandValidator : AbstractValidator<DeleteRelationshipCommand>
{
    public DeleteRelationshipCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID mối quan hệ không được để trống.");
    }
}
