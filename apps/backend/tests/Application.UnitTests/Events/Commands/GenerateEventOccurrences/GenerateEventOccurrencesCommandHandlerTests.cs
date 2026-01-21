using backend.Application.Events.Commands.GenerateEventOccurrences;
using backend.Application.Events.EventOccurrences.Jobs; // Needed for mocking
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.GenerateEventOccurrences;

public class GenerateEventOccurrencesCommandHandlerTests : TestBase
{
    private readonly Mock<IGenerateEventOccurrencesJob> _mockGenerateEventOccurrencesJob;
    private readonly Mock<ILogger<GenerateEventOccurrencesCommandHandler>> _mockLogger;
    private readonly GenerateEventOccurrencesCommandHandler _handler;

    public GenerateEventOccurrencesCommandHandlerTests()
    {
        _mockGenerateEventOccurrencesJob = new Mock<IGenerateEventOccurrencesJob>();
        _mockLogger = new Mock<ILogger<GenerateEventOccurrencesCommandHandler>>();

        _handler = new GenerateEventOccurrencesCommandHandler(
            _mockGenerateEventOccurrencesJob.Object,
            _mockLogger.Object,
            _mockAuthorizationService.Object // Use _mockAuthorizationService from TestBase
        );
    }

    [Fact]
    public async Task Handle_ShouldCallGenerateOccurrencesWithCorrectParameters_WhenFamilyIdProvided()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        var command = new GenerateEventOccurrencesCommand { Year = year, FamilyId = familyId };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin

        _mockGenerateEventOccurrencesJob
            .Setup(j => j.GenerateOccurrences(year, familyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain($"Event occurrences generated directly for year {year} and FamilyId {familyId}.");
        _mockGenerateEventOccurrencesJob.Verify(
            j => j.GenerateOccurrences(year, familyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyIdIsNotProvided()
    {
        // Arrange
        var year = 2024;
        var command = new GenerateEventOccurrencesCommand { Year = year, FamilyId = null };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Generating occurrences directly requires a specific FamilyId.");
        _mockGenerateEventOccurrencesJob.Verify(
            j => j.GenerateOccurrences(It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAdmin()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        var command = new GenerateEventOccurrencesCommand { Year = year, FamilyId = familyId };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // User is NOT admin

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Access Denied: Only administrators can generate event occurrences directly.");
        _mockGenerateEventOccurrencesJob.Verify(
            j => j.GenerateOccurrences(It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never); // Job should not be called
    }
}
