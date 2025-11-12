using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.Faces.Queries;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandValidatorTests
{
    private readonly SaveFaceLabelsCommandValidator _validator;

    public SaveFaceLabelsCommandValidatorTests()
    {
        _validator = new SaveFaceLabelsCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenImageIdIsEmpty()
    {
        var command = new SaveFaceLabelsCommand { ImageId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
              .WithErrorMessage("ImageId is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenFaceLabelsIsNull()
    {
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FaceLabels)
              .WithErrorMessage("FaceLabels list cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFaceLabelsIsEmpty()
    {
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto>() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FaceLabels)
              .WithErrorMessage("FaceLabels list cannot be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenDetectedFaceIdIsEmpty()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = "", MemberId = Guid.NewGuid(), Embedding = new List<double> { 0.1 } }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Id")
              .WithErrorMessage("DetectedFaceDto Id is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenMemberIdIsNull()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = Guid.NewGuid().ToString(), MemberId = null, Embedding = new List<double> { 0.1 } }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].MemberId")
              .WithErrorMessage("DetectedFaceDto MemberId is required for labeled faces.");
    }

    [Fact]
    public void ShouldHaveError_WhenMemberIdIsEmptyGuid()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = Guid.NewGuid().ToString(), MemberId = Guid.Empty, Embedding = new List<double> { 0.1 } }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].MemberId")
              .WithErrorMessage("DetectedFaceDto MemberId is required for labeled faces.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmbeddingIsNull()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = Guid.NewGuid().ToString(), MemberId = Guid.NewGuid(), Embedding = null }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Embedding")
              .WithErrorMessage("'Embedding' must not be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenEmbeddingIsEmpty()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = Guid.NewGuid().ToString(), MemberId = Guid.NewGuid(), Embedding = new List<double> { } }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Embedding")
              .WithErrorMessage("DetectedFaceDto Embedding cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new SaveFaceLabelsCommand
        {
            ImageId = Guid.NewGuid(),
            FaceLabels = new List<DetectedFaceDto>
            {
                new DetectedFaceDto { Id = Guid.NewGuid().ToString(), MemberId = Guid.NewGuid(), Embedding = new List<double> { 0.1, 0.2 } }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
