using System.Linq; // For .Any()
using backend.Application.AI.DTOs; // NEW USING
using FluentValidation;

namespace backend.Application.AI.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandValidator : AbstractValidator<AnalyzePhotoCommand>
{
    public AnalyzePhotoCommandValidator()
    {
        RuleFor(v => v.Input)
            .NotNull().WithMessage("Input data is required.");

        When(v => v.Input != null, () =>
        {
            RuleFor(v => v.Input.ImageBase64)
                .NotEmpty().When(v => string.IsNullOrEmpty(v.Input.ImageUrl)).WithMessage("Either ImageBase64 or ImageUrl must be provided.");

            RuleFor(v => v.Input.ImageUrl)
                .NotEmpty().When(v => string.IsNullOrEmpty(v.Input.ImageBase64)).WithMessage("Either ImageBase64 or ImageUrl must be provided.");

            RuleFor(v => v.Input.Faces)
                .NotNull().WithMessage("Faces data is required.")
                .Must(faces => faces != null && faces.Any()).WithMessage("At least one face must be detected.");

            RuleFor(v => v.Input.TargetFaceId)
                .NotEmpty().WithMessage("TargetFaceId is required when faces are detected.")
                .Must((command, targetFaceId) => command.Input.Faces == null || command.Input.Faces.Any(f => f.FaceId == targetFaceId))
                .WithMessage("TargetFaceId must correspond to a detected face.");

            RuleForEach(v => v.Input.Faces).SetValidator(new AiDetectedFaceDtoValidator());
        });
    }
}

public class AiDetectedFaceDtoValidator : AbstractValidator<DTOs.AiDetectedFaceDto>
{
    public AiDetectedFaceDtoValidator()
    {
        RuleFor(x => x.FaceId)
            .NotEmpty().WithMessage("FaceId is required.");

        RuleFor(x => x.Bbox)
            .NotNull().WithMessage("Bounding box is required.")
            .Must(bbox => bbox != null && bbox.Count == 4).WithMessage("Bounding box must contain 4 integer values (x, y, w, h).");

        RuleFor(x => x.EmotionLocal)
            .NotNull().WithMessage("Emotion data is required.");

        When(x => x.EmotionLocal != null, () =>
        {
            RuleFor(x => x.EmotionLocal.Dominant)
                .NotEmpty().WithMessage("Dominant emotion is required.");

            RuleFor(x => x.EmotionLocal.Confidence)
                .GreaterThanOrEqualTo(0).LessThanOrEqualTo(1).WithMessage("Emotion confidence must be between 0 and 1.");
        });
    }
}
