using backend.Application.Common.Models;
using backend.Application.AI.Commands.AnalyzePhoto;
using backend.Application.Memories.Commands.CreateMemory;
using backend.Application.Memories.Commands.DeleteMemory;
using backend.Application.Memories.Commands.GenerateStory;
using backend.Application.Memories.Commands.UpdateMemory;
using backend.Application.Memories.DTOs;
using backend.Application.Memories.Queries.GetMemoriesByMemberId;
using backend.Application.Memories.Queries.GetMemoryDetail;
using backend.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Web.UnitTests.Controllers;

public class MemoriesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MemoriesController _controller;

    public MemoriesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        // The MemoriesController constructor now only takes IMediator
        // IPhotoAnalysisService and IStoryGenerationService are not directly injected
        _controller = new MemoriesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task CreateMemory_ShouldReturnCreatedAtAction_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateMemoryCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "Test Memory",
            Story = "This is a test story.",
        };
        var expectedMemoryId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(expectedMemoryId));

        // Act
        var result = await _controller.CreateMemory(command, CancellationToken.None);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(MemoriesController.GetMemoryDetail), createdAtActionResult.ActionName);
        Assert.Equal(expectedMemoryId, createdAtActionResult.Value);
    }

    [Fact]
    public async Task CreateMemory_ShouldReturnBadRequest_WhenCommandIsInvalid()
    {
        // Arrange
        var command = new CreateMemoryCommand
        {
            MemberId = Guid.Empty, // Invalid
            Title = "", // Invalid
            Story = "", // Invalid
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Validation failed"));

        // Act
        var result = await _controller.CreateMemory(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateMemory_ShouldReturnNoContent_WhenCommandIsValid()
    {
        // Arrange
        var memoryId = Guid.NewGuid();
        var command = new UpdateMemoryCommand
        {
            Id = memoryId,
            MemberId = Guid.NewGuid(),
            Title = "Updated Title",
            Story = "Updated Story",
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateMemoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.UpdateMemory(memoryId, command, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateMemory_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange
        var memoryId = Guid.NewGuid();
        var command = new UpdateMemoryCommand
        {
            Id = Guid.NewGuid(), // Mismatched ID
            MemberId = Guid.NewGuid(),
            Title = "Updated Title",
            Story = "Updated Story",
        };

        // Act
        var result = await _controller.UpdateMemory(memoryId, command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteMemory_ShouldReturnNoContent_WhenCommandIsValid()
    {
        // Arrange
        var memoryId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteMemoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.DeleteMemory(memoryId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetMemoryDetail_ShouldReturnMemoryDto_WhenMemoryExists()
    {
        // Arrange
        var memoryId = Guid.NewGuid();
        var expectedDto = new MemoryDto { Id = memoryId, Title = "Detail Memory" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMemoryDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryDto>.Success(expectedDto));

        // Act
        var result = await _controller.GetMemoryDetail(memoryId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualDto = Assert.IsType<MemoryDto>(okResult.Value);
        Assert.Equal(expectedDto.Id, actualDto.Id);
    }

    [Fact]
    public async Task GetMemoryDetail_ShouldReturnNotFound_WhenMemoryDoesNotExist()
    {
        // Arrange
        var memoryId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMemoryDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryDto>.Failure("Not Found"));

        // Act
        var result = await _controller.GetMemoryDetail(memoryId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMemoriesByMemberId_ShouldReturnPaginatedList_WhenMemoriesExist()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var expectedList = new PaginatedList<MemoryDto>(new List<MemoryDto> { new MemoryDto() }, 1, 1, 10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMemoriesByMemberIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedList<MemoryDto>>.Success(expectedList));

        // Act
        var result = await _controller.GetMemoriesByMemberId(memberId, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualList = Assert.IsType<PaginatedList<MemoryDto>>(okResult.Value);
        Assert.Equal(expectedList.TotalCount, actualList.TotalCount);
    }

    [Fact]
    public async Task AnalyzePhoto_ShouldReturnPhotoAnalysisResultDto_WhenSuccessful()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            File = new FormFile(new MemoryStream(), 0, 100, "Data", "test.jpg"),
            MemberId = Guid.NewGuid()
        };
        var expectedDto = new PhotoAnalysisResultDto { Id = Guid.NewGuid(), Summary = "Analyzed" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<AnalyzePhotoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PhotoAnalysisResultDto>.Success(expectedDto));

        // Act
        var result = await _controller.AnalyzePhoto(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualDto = Assert.IsType<PhotoAnalysisResultDto>(okResult.Value);
        Assert.Equal(expectedDto.Id, actualDto.Id);
    }

    [Fact]
    public async Task AnalyzePhoto_ShouldReturnBadRequest_WhenAnalysisFails()
    {
        // Arrange
        var command = new AnalyzePhotoCommand
        {
            File = new FormFile(new MemoryStream(), 0, 100, "Data", "test.jpg"),
            MemberId = Guid.NewGuid()
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<AnalyzePhotoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PhotoAnalysisResultDto>.Failure("Analysis Failed"));

        // Act
        var result = await _controller.AnalyzePhoto(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GenerateStory_ShouldReturnGenerateStoryResponseDto_WhenSuccessful()
    {
        // Arrange
        var command = new GenerateStoryCommand
        {
            MemberId = Guid.NewGuid(),
            RawText = "Some raw text",
            Style = "nostalgic"
        };
        var expectedDto = new GenerateStoryResponseDto { Title = "Generated Story", Story = "..." };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GenerateStoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GenerateStoryResponseDto>.Success(expectedDto));

        // Act
        var result = await _controller.GenerateStory(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualDto = Assert.IsType<GenerateStoryResponseDto>(okResult.Value);
        Assert.Equal(expectedDto.Title, actualDto.Title);
    }

    [Fact]
    public async Task GenerateStory_ShouldReturnBadRequest_WhenGenerationFails()
    {
        // Arrange
        var command = new GenerateStoryCommand
        {
            MemberId = Guid.NewGuid(),
            RawText = "Some raw text",
            Style = "nostalgic"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GenerateStoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GenerateStoryResponseDto>.Failure("Generation Failed"));

        // Act
        var result = await _controller.GenerateStory(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
