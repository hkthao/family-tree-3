namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public class CreateMemberFaceCommandValidator : AbstractValidator<CreateMemberFaceCommand>
{
    public CreateMemberFaceCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");



        RuleFor(v => v.Embedding)
            .NotNull().WithMessage("Embedding cannot be null.")
            .Must(list => list != null && list.Any()).WithMessage("Embedding cannot be empty.");

        RuleFor(v => v.BoundingBox)
            .NotNull().WithMessage("BoundingBox cannot be null.");
    }
}
