using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Prompts.Commands.ImportPrompts;
using backend.Application.Prompts.DTOs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Prompts.Commands;

public class ImportPromptsCommandHandlerTests : TestBase
{
    private readonly ImportPromptsCommandHandler _handler;
    private readonly Mock<ILogger<ImportPromptsCommandHandler>> _mockLogger;

    public ImportPromptsCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ImportPromptsCommandHandler>>();

        _handler = new ImportPromptsCommandHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldImportNewPrompts_WhenUserIsAdminAndPromptsAreNew()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);


        var importItems = new List<ImportPromptItemDto>
        {
            new() { Code = "NEW_CODE_1", Title = "New Title 1", Content = "New Content 1", Description = "New Desc 1" },
            new() { Code = "NEW_CODE_2", Title = "New Title 2", Content = "New Content 2", Description = "New Desc 2" }
        };
        var command = new ImportPromptsCommand(importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);

        _context.Prompts.Should().HaveCount(2);
        _context.Prompts.First().Code.Should().Be("NEW_CODE_1");
        _context.Prompts.Last().Code.Should().Be("NEW_CODE_2");
    }

    [Fact]
    public async Task Handle_ShouldSkipExistingPrompts_WhenUserIsAdminAndPromptsExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);



        await _context.Prompts.AddAsync(new Prompt { Code = "EXISTING_CODE", Title = "Existing Title", Content = "Existing Content" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportPromptItemDto>
        {
            new() { Code = "EXISTING_CODE", Title = "Updated Title", Content = "Updated Content" }, // This should be skipped
            new() { Code = "NEW_CODE", Title = "New Title", Content = "New Content" }
        };
        var command = new ImportPromptsCommand(importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1); // Only the new prompt should be imported

        _context.Prompts.Should().HaveCount(2); // Existing + 1 new
        _context.Prompts.Any(p => p.Code == "EXISTING_CODE").Should().BeTrue();
        _context.Prompts.Any(p => p.Code == "NEW_CODE").Should().BeTrue();
        _context.Prompts.First(p => p.Code == "EXISTING_CODE").Title.Should().Be("Existing Title"); // Should not be updated
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var importItems = new List<ImportPromptItemDto>
        {
            new() { Code = "NEW_CODE", Title = "New Title", Content = "New Content" }
        };
        var command = new ImportPromptsCommand(importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.Prompts.Should().BeEmpty(); // No prompts should be imported
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenImportingEmptyList()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var command = new ImportPromptsCommand(new List<ImportPromptItemDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _context.Prompts.Should().BeEmpty();
    }
}