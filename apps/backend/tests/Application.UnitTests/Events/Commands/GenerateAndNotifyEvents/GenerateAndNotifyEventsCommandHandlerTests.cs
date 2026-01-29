using backend.Application.Common.Constants;
using backend.Application.Events.Commands.GenerateAndNotifyEvents;
using backend.Application.Events.EventOccurrences.Jobs;
using backend.Application.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.GenerateAndNotifyEvents;

public class GenerateAndNotifyEventsCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<GenerateAndNotifyEventsCommandHandler>> _mockLogger;
    private readonly Mock<IGenerateEventOccurrencesJob> _mockGenerateEventOccurrencesJob;
    private readonly Mock<IEventNotificationJob> _mockEventNotificationJob;
    private readonly GenerateAndNotifyEventsCommandHandler _handler;

    public GenerateAndNotifyEventsCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GenerateAndNotifyEventsCommandHandler>>();
        _mockGenerateEventOccurrencesJob = new Mock<IGenerateEventOccurrencesJob>();
        _mockEventNotificationJob = new Mock<IEventNotificationJob>();

        _handler = new GenerateAndNotifyEventsCommandHandler(
            _mockLogger.Object,
            _mockAuthorizationService.Object, // Use inherited mock
            _mockGenerateEventOccurrencesJob.Object,
            _mockEventNotificationJob.Object,
            _mockDateTime.Object); // Use inherited mock
    }

    [Fact]
    public async Task Handle_GivenNonAdminUser_ReturnsFailureResult()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        var command = new GenerateAndNotifyEventsCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Access Denied: Only administrators can trigger this operation.", result.Error);
        Assert.Equal(ErrorSources.Forbidden, result.ErrorSource);
        _mockGenerateEventOccurrencesJob.Verify(x => x.GenerateOccurrences(It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEventNotificationJob.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GivenAdminUserWithFamilyIdAndYear_CallsJobsWithCorrectArgumentsAndReturnsSuccess()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var familyId = Guid.NewGuid();
        var year = 2024;
        var command = new GenerateAndNotifyEventsCommand { FamilyId = familyId, Year = year };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains($"Successfully generated occurrences for year {year} and triggered notifications for FamilyId: {familyId}.", result.Value);
        _mockGenerateEventOccurrencesJob.Verify(x => x.GenerateOccurrences(year, familyId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventNotificationJob.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenAdminUserWithoutYear_UsesCurrentYearAndCallsJobsWithCorrectArgumentsAndReturnsSuccess()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var familyId = Guid.NewGuid();
        var currentYear = 2025; // Mock current year
        _mockDateTime.Setup(x => x.Now).Returns(new DateTime(currentYear, 1, 1));
        var command = new GenerateAndNotifyEventsCommand { FamilyId = familyId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains($"Successfully generated occurrences for year {currentYear} and triggered notifications for FamilyId: {familyId}.", result.Value);
        _mockGenerateEventOccurrencesJob.Verify(x => x.GenerateOccurrences(currentYear, familyId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventNotificationJob.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenAdminUserWithoutFamilyId_CallsJobsWithCorrectArgumentsAndReturnsSuccess()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var year = 2024;
        var command = new GenerateAndNotifyEventsCommand { Year = year };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains($"Successfully generated occurrences for year {year} and triggered notifications for FamilyId: .", result.Value); // FamilyId will be null
        _mockGenerateEventOccurrencesJob.Verify(x => x.GenerateOccurrences(year, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventNotificationJob.Verify(x => x.Run(It.IsAny<CancellationToken>()), Times.Once);
    }
}
