using AutoFixture;
using backend.Application.AI.Chunk.EmbedChunks;
using backend.Domain.Entities;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.AI.Chunk.EmbedChunks;

public class EmbedChunksCommandValidatorTests
{
    private readonly Fixture _fixture;
    private readonly EmbedChunksCommandValidator _validator;

    public EmbedChunksCommandValidatorTests()
    {
        _fixture = new Fixture();
        _validator = new EmbedChunksCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenChunksIsNull()
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Chunks)
            .WithErrorMessage("Chunks cannot be null.");
    }

    [Fact]
    public void ShouldHaveError_WhenChunksIsEmpty()
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = new List<TextChunk>() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Chunks)
            .WithErrorMessage("Chunks cannot be empty.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenChunksIsProvided()
    {
        // Arrange
        var command = new EmbedChunksCommand { Chunks = _fixture.CreateMany<TextChunk>(1).ToList() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Chunks);
    }
}
