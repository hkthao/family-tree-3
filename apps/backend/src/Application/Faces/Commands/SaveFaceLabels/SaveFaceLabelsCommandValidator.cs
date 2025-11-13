namespace backend.Application.Faces.Commands.SaveFaceLabels
{
    public class SaveFaceLabelsCommandValidator : AbstractValidator<SaveFaceLabelsCommand>
    {
        public SaveFaceLabelsCommandValidator()
        {
            RuleFor(v => v.ImageId)
                .NotEmpty().WithMessage("ImageId is required.");

            RuleFor(v => v.FaceLabels)
                .NotNull().WithMessage("FaceLabels list cannot be null.")
                .Must(list => list == null || list.Any()).WithMessage("FaceLabels list cannot be empty.");

            RuleForEach(v => v.FaceLabels).Cascade(CascadeMode.Stop).ChildRules(face =>
            {
                face.RuleFor(f => f.Id)
                    .NotEmpty().WithMessage("DetectedFaceDto Id is required.");

                face.RuleFor(f => f.MemberId)
                    .NotNull().WithMessage("DetectedFaceDto MemberId is required for labeled faces.")
                    .Must(id => id != Guid.Empty).WithMessage("DetectedFaceDto MemberId is required for labeled faces.");

                face.RuleFor(f => f.Embedding)
                    .Cascade(CascadeMode.Stop)
                    .NotNull().NotEmpty().WithMessage("DetectedFaceDto Embedding cannot be empty.");
            });
        }
    }
}
