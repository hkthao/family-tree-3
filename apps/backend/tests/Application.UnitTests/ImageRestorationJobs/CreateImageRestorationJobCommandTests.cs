using backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;
using backend.Application.ImageRestorationJobs.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace backend.Application.UnitTests.ImageRestorationJobs;

public class CreateImageRestorationJobCommandTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateImageRestorationJobCommandHandler>> _loggerMock;
    private readonly DateTime _fixedDateTime; // To ensure consistent time for Now

    public CreateImageRestorationJobCommandTests()
    {
        _currentUserMock = _mockUser; // Use the mock from TestBase
        _loggerMock = new Mock<ILogger<CreateImageRestorationJobCommandHandler>>();

        _fixedDateTime = DateTime.Now; // Fix the DateTime for the test run
        _mockDateTime.Setup(d => d.Now).Returns(_fixedDateTime); // Set the Now property of TestBase's _mockDateTime

        // The UserId is set by TestBase, we just ensure IsAuthenticated is true for these tests
        _currentUserMock.Setup(s => s.IsAuthenticated).Returns(true);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ShouldCreateImageRestorationJob()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            OriginalImageUrl: "http://example.com/original.jpg",
            FamilyId: Guid.NewGuid()
        );

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object,
            _loggerMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<ImageRestorationJobDto>(result.Value);

        var createdJob = _context.ImageRestorationJobs.FirstOrDefault(j => j.OriginalImageUrl == command.OriginalImageUrl);
        Assert.NotNull(createdJob);
        Assert.Equal(command.OriginalImageUrl, createdJob.OriginalImageUrl);
        Assert.Equal(command.FamilyId, createdJob.FamilyId);
        Assert.Equal(RestorationStatus.Processing, createdJob.Status);
        Assert.Equal(_currentUserMock.Object.UserId.ToString(), createdJob.CreatedBy);
        Assert.Equal(_fixedDateTime, createdJob.Created);
    }

    [Fact]
    public async Task Handle_GivenUnauthenticatedUser_ShouldReturnFailure()
    {
        // Arrange
        _currentUserMock.Setup(s => s.UserId).Returns(Guid.Empty);
        _currentUserMock.Setup(s => s.IsAuthenticated).Returns(false);

        var command = new CreateImageRestorationJobCommand(
            OriginalImageUrl: "http://example.com/original.jpg",
            FamilyId: Guid.NewGuid()
        );

        var handler = new CreateImageRestorationJobCommandHandler(
            _context,
            _mapper,
            _currentUserMock.Object,
            _mockDateTime.Object, // Use _mockDateTime from TestBase
            _loggerMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User is not authenticated.", result.Error);
    }

    [Fact]
    public async Task Handle_GivenEmptyOriginalImageUrl_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            OriginalImageUrl: "", // Empty URL
            FamilyId: Guid.NewGuid()
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Original Image URL is required.", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenInvalidOriginalImageUrl_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            OriginalImageUrl: "not-a-valid-url", // Invalid URL
            FamilyId: Guid.NewGuid()
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Original Image URL must be a valid URL.", validationResult.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Handle_GivenEmptyFamilyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateImageRestorationJobCommand(
            OriginalImageUrl: "http://example.com/original.jpg",
            FamilyId: Guid.Empty
        );

        var validator = new CreateImageRestorationJobCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains("Family ID is required.", validationResult.Errors.Select(e => e.ErrorMessage));
    }
}

