using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;
    private readonly Mock<IValidator<FamilyDto>> _familyDtoValidatorMock;
    private readonly GenerateFamilyDataCommandHandler _handler;

    public GenerateFamilyDataCommandHandlerTests()
    {
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();
        _familyDtoValidatorMock = new Mock<IValidator<FamilyDto>>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);

        _handler = new GenerateFamilyDataCommandHandler(
            _chatProviderFactoryMock.Object,
            _familyDtoValidatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateFamilyData_WhenValidCommandAndAIResponseIsValid()
    {
        // Arrange
        var aiResponseJson = "{\"families\":[{\"name\":\"Test Family\",\"description\":\"A test family\",\"address\":\"Test Address\",\"visibility\":\"Public\",\"avatarUrl\":\"http://example.com/avatar.png\",\"totalMembers\":10,\"totalGenerations\":3}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _familyDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

        var command = new GenerateFamilyDataCommand("Generate a test family.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var familyDto = result.Value!.First();
        familyDto.Name.Should().Be("Test Family");
        familyDto.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // Arrange
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(" ");
        var command = new GenerateFamilyDataCommand("Generate empty response.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.NoAIResponse);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAIResponseContainsNoFamilies()
    {
        // Arrange
        var aiResponseJson = "{\"families\":[]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        var command = new GenerateFamilyDataCommand("Generate no families.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync("invalid json");
        var command = new GenerateFamilyDataCommand("Generate invalid json.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().StartWith(string.Format(ErrorMessages.InvalidAIResponse, ""));
    }

    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenFamilyDtoValidationFails()
    {
        // Arrange
        var validationError = "Name is required.";
        var aiResponseJson = "{\"families\":[{\"name\":\"\"}] }"; // Empty name
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _familyDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", validationError) }));
        var command = new GenerateFamilyDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(validationError);
    }

    [Fact]
    public async Task Handle_ShouldDefaultVisibilityToPublic_WhenVisibilityIsMissing()
    {
        // Arrange
        var aiResponseJson = "{\"families\":[{\"name\":\"Family without visibility\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _familyDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateFamilyDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().Visibility.Should().Be("Public");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var exceptionMessage = "Unexpected error.";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ThrowsAsync(new Exception(exceptionMessage));
        var command = new GenerateFamilyDataCommand("Test");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
    }
}