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
                .NotEmpty().When(v => string.IsNullOrEmpty(v.Input.ImageUrl)).WithMessage("Either ImageBase64 or ImageUrl must be provided.")
                .MaximumLength(500000).WithMessage("ImageBase64 must not exceed 500,000 characters."); // Roughly 375KB

            RuleFor(v => v.Input.ImageUrl)
                .NotEmpty().When(v => string.IsNullOrEmpty(v.Input.ImageBase64)).WithMessage("Either ImageBase64 or ImageUrl must be provided.")
                .MaximumLength(2048).WithMessage("ImageUrl must not exceed 2048 characters."); // Standard URL max length

            RuleFor(v => v.Input.TargetFaceCropUrl)
                .MaximumLength(2048).WithMessage("TargetFaceCropUrl must not exceed 2048 characters.")
                .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("TargetFaceCropUrl must be a valid URL.");

            RuleFor(v => v.Input.Faces)
                .NotNull().WithMessage("Faces data is required.")
                .Must(faces => faces != null && faces.Any()).WithMessage("At least one face must be detected.");

            RuleFor(v => v.Input.TargetFaceId)
                .NotEmpty().WithMessage("TargetFaceId is required when faces are detected.")
                .Must((command, targetFaceId) => command.Input.Faces == null || command.Input.Faces.Any(f => f.FaceId == targetFaceId))
                .WithMessage("TargetFaceId must correspond to a detected face.");

            When(v => v.Input.Exif != null, () =>
            {
                RuleFor(v => v.Input.Exif!.Datetime)
                    .MaximumLength(256).WithMessage("Exif datetime must not exceed 256 characters.");
                RuleFor(v => v.Input.Exif!.Gps)
                    .MaximumLength(256).WithMessage("Exif GPS data must not exceed 256 characters.");
                RuleFor(v => v.Input.Exif!.CameraInfo)
                    .MaximumLength(256).WithMessage("Exif camera info must not exceed 256 characters.");
            });

            When(v => v.Input.MemberInfo != null, () =>
            {
                RuleFor(v => v.Input.MemberInfo!.Name)
                    .MaximumLength(256).WithMessage("Member name must not exceed 256 characters.");
            });

            RuleForEach(v => v.Input.Faces).SetValidator(new AiDetectedFaceDtoValidator());
        });
    }
}

public class AiDetectedFaceDtoValidator : AbstractValidator<DTOs.AiDetectedFaceDto>
{
    public AiDetectedFaceDtoValidator()
    {
        RuleFor(x => x.FaceId)
            .NotEmpty().WithMessage("FaceId is required.")
            .MaximumLength(256).WithMessage("FaceId must not exceed 256 characters.");

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
