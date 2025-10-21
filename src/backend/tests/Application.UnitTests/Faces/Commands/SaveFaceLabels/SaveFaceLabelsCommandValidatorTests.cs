using backend.Application.Faces.Commands.SaveFaceLabels;
using FluentValidation.TestHelper;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using backend.Application.Faces.Queries;
using backend.Application.Faces.Common;

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
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ImageId là Guid rỗng.
        var command = new SaveFaceLabelsCommand { ImageId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
              .WithErrorMessage("ImageId is required.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenImageIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi ImageId hợp lệ.
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ImageId);
    }

    [Fact]
    public void ShouldHaveError_WhenFaceLabelsIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FaceLabels là null.
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FaceLabels)
              .WithErrorMessage("FaceLabels list cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenFaceLabelsIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FaceLabels là danh sách rỗng.
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto>() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FaceLabels)
              .WithErrorMessage("FaceLabels list cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFaceLabelsIsNotEmptyAndValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FaceLabels không rỗng và hợp lệ.
        var validFace = new DetectedFaceDto
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = Guid.NewGuid(),
            Embedding = new List<double> { 0.1, 0.2 }
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { validFace } };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FaceLabels);
    }

    [Fact]
    public void ShouldHaveError_WhenDetectedFaceIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DetectedFaceDto Id là chuỗi rỗng.
        var invalidFace = new DetectedFaceDto
        {
            Id = string.Empty,
            MemberId = Guid.NewGuid(),
            Embedding = new List<double> { 0.1, 0.2 }
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { invalidFace } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Id")
              .WithErrorMessage("DetectedFaceDto Id is required.");
    }

    [Fact]
    public void ShouldHaveError_WhenDetectedFaceMemberIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DetectedFaceDto MemberId là Guid rỗng.
        var invalidFace = new DetectedFaceDto
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = Guid.Empty,
            Embedding = new List<double> { 0.1, 0.2 }
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { invalidFace } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].MemberId")
              .WithErrorMessage("DetectedFaceDto MemberId is required for labeled faces.");
    }

    [Fact]
    public void ShouldHaveError_WhenDetectedFaceEmbeddingIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DetectedFaceDto Embedding là null.
        var invalidFace = new DetectedFaceDto
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = Guid.NewGuid(),
            Embedding = null
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { invalidFace } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Embedding")
              .WithErrorMessage("'Embedding' must not be empty.");
    }

    [Fact]
    public void ShouldHaveError_WhenDetectedFaceEmbeddingIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DetectedFaceDto Embedding là danh sách rỗng.
        var invalidFace = new DetectedFaceDto
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = Guid.NewGuid(),
            Embedding = new List<double>()
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { invalidFace } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("FaceLabels[0].Embedding")
              .WithErrorMessage("DetectedFaceDto Embedding cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenDetectedFaceIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi DetectedFaceDto hợp lệ.
        var validFace = new DetectedFaceDto
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = Guid.NewGuid(),
            Embedding = new List<double> { 0.1, 0.2 }
        };
        var command = new SaveFaceLabelsCommand { ImageId = Guid.NewGuid(), FaceLabels = new List<DetectedFaceDto> { validFace } };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
