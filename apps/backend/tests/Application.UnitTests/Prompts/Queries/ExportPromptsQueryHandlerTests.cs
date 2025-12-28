using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Prompts.DTOs;
using backend.Application.Prompts.Queries.ExportPrompts;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Prompts.Queries;

public class ExportPromptsQueryHandlerTests : TestBase
{
    private readonly ExportPromptsQueryHandler _handler;
    private readonly Mock<ILogger<ExportPromptsQueryHandler>> _mockLogger;

    public ExportPromptsQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ExportPromptsQueryHandler>>();

        _handler = new ExportPromptsQueryHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldExportAllPrompts_WhenUserIsAdminAndPromptsExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        await _context.Prompts.AddAsync(new Prompt { Code = "CODE_1", Title = "Title 1", Content = "Content 1" });
        await _context.Prompts.AddAsync(new Prompt { Code = "CODE_2", Title = "Title 2", Content = "Content 2" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportPromptsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
        result.Value.Should().Contain(p => p.Code == "CODE_1");
        result.Value.Should().Contain(p => p.Code == "CODE_2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIsAdminAndNoPromptsExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        // No prompts added to context

        var query = new ExportPromptsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        await _context.Prompts.AddAsync(new Prompt { Code = "CODE_1", Title = "Title 1", Content = "Content 1" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportPromptsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
