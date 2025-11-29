using backend.Application.AI.Commands.AnalyzePhoto;
using backend.Application.AI.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandValidatorTests
{
    private readonly AnalyzePhotoCommandValidator _validator;

    public AnalyzePhotoCommandValidatorTests()
    {
        _validator = new AnalyzePhotoCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenInputIsNull()
    {
        var command = new AnalyzePhotoCommand { Input = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input)
              .WithErrorMessage("Input data is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenNeitherImageBase64NorImageUrlIsProvided()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "",
                ImageUrl = "",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = Guid.NewGuid().ToString()
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.ImageBase64)
              .WithErrorMessage("Either ImageBase64 or ImageUrl must be provided.");
        result.ShouldHaveValidationErrorFor(x => x.Input.ImageUrl)
              .WithErrorMessage("Either ImageBase64 or ImageUrl must be provided.");
    }

    [Fact]
    public void ShouldHaveError_WhenImageBase64IsTooLong()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = new string('a', 500001),
                ImageUrl = "http://valid.url/image.jpg",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = Guid.NewGuid().ToString()
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.ImageBase64)
              .WithErrorMessage("ImageBase64 must not exceed 500,000 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenImageUrlIsTooLong()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                ImageUrl = "http://valid.url/" + new string('a', 2030) + ".jpg",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.ImageUrl)
              .WithErrorMessage("ImageUrl must not exceed 2048 characters.");
    }

    [Fact]
    public void ShouldHaveError_WhenTargetFaceCropUrlIsInvalid()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                TargetFaceCropUrl = "invalid-url",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = Guid.NewGuid().ToString()
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.TargetFaceCropUrl)
              .WithErrorMessage("TargetFaceCropUrl must be a valid URL.");
    }

    [Fact]
    public void ShouldHaveError_WhenFacesIsNull()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = null!,
                TargetFaceId = Guid.NewGuid().ToString()
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.Faces)
              .WithErrorMessage("Faces data is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenFacesIsEmpty()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto>(),
                TargetFaceId = Guid.NewGuid().ToString()
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.Faces)
              .WithErrorMessage("At least one face must be detected.");
    }

    [Fact]
    public void ShouldHaveError_WhenTargetFaceIdIsEmptyAndFacesExist()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = ""
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.TargetFaceId)
              .WithErrorMessage("TargetFaceId is required when faces are detected.");
    }

    [Fact]
    public void ShouldHaveError_WhenTargetFaceIdDoesNotCorrespondToDetectedFace()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "nonexistent_face"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Input.TargetFaceId)
              .WithErrorMessage("TargetFaceId must correspond to a detected face.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenFaceIdIsEmpty()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1" // TargetFaceId can be anything if FaceId itself is invalid
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Input.Faces[0].FaceId")
              .WithErrorMessage("FaceId is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenBboxIsNull()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = null!, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Input.Faces[0].Bbox")
              .WithErrorMessage("Bounding box is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenBboxCountIsNotFour()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Input.Faces[0].Bbox")
              .WithErrorMessage("Bounding box must contain 4 integer values (x, y, w, h).");
    }

    [Fact]
    public void ShouldHaveError_WhenEmotionLocalDominantIsEmpty()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "", Confidence = 0.9 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Input.Faces[0].EmotionLocal.Dominant")
              .WithErrorMessage("Dominant emotion is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmotionLocalConfidenceIsOutOfRange()
    {
        var command = new AnalyzePhotoCommand
        {
            Input = new AiPhotoAnalysisInputDto
            {
                ImageBase64 = "base64",
                Faces = new List<AiDetectedFaceDto> { new AiDetectedFaceDto { FaceId = "face1", Bbox = new List<int> { 0, 0, 10, 10 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 1.1 } } },
                TargetFaceId = "face1"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Input.Faces[0].EmotionLocal.Confidence")
              .WithErrorMessage("Emotion confidence must be between 0 and 1.");
    }
}
