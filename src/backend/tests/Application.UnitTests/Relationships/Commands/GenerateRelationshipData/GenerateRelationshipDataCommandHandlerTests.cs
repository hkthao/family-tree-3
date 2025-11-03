using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.GenerateRelationshipData;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.GenerateRelationshipData;

public class GenerateRelationshipDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;
    private readonly Mock<IValidator<AIRelationshipDto>> _validatorMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILogger<GenerateRelationshipDataCommandHandler>> _loggerMock;

    public GenerateRelationshipDataCommandHandlerTests()
    {
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();
        _validatorMock = new Mock<IValidator<AIRelationshipDto>>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<GenerateRelationshipDataCommandHandler>>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithValidJson()
    {
        // Arrange
        var json = "{\"relationships\":[{\"sourceMemberName\":\"John Doe\",\"targetMemberName\":\"Jane Doe\",\"type\":\"Husband\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(json);
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new GenerateRelationshipDataCommandHandler(_chatProviderFactoryMock.Object, _validatorMock.Object, _context, _authorizationServiceMock.Object, _loggerMock.Object);
        var command = new GenerateRelationshipDataCommand("test prompt");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WithInvalidJson()
    {
        // Arrange
        var json = "invalid json";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(json);

        var handler = new GenerateRelationshipDataCommandHandler(_chatProviderFactoryMock.Object, _validatorMock.Object, _context, _authorizationServiceMock.Object, _loggerMock.Object);
        var command = new GenerateRelationshipDataCommand("test prompt");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiReturnsNoResponse()
    {
        // Arrange
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(string.Empty);

        var handler = new GenerateRelationshipDataCommandHandler(_chatProviderFactoryMock.Object, _validatorMock.Object, _context, _authorizationServiceMock.Object, _loggerMock.Object);
        var command = new GenerateRelationshipDataCommand("test prompt");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithValidationErrors_WhenMemberNotFound()
    {
        // Arrange
        var json = "{\"relationships\":[{\"sourceMemberName\":\"John Doe\",\"targetMemberName\":\"Jane Doe\",\"type\":\"Husband\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(json);
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new GenerateRelationshipDataCommandHandler(_chatProviderFactoryMock.Object, _validatorMock.Object, _context, _authorizationServiceMock.Object, _loggerMock.Object);
        var command = new GenerateRelationshipDataCommand("test prompt");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().NotBeEmpty();
    }
}
