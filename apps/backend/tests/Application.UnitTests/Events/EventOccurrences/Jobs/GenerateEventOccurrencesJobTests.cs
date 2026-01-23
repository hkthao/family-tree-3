using backend.Application.Common.Services;
using backend.Application.Events.EventOccurrences.Jobs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Keep for Mock<ILogger> in constructor
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.EventOccurrences.Jobs;

public class GenerateEventOccurrencesJobTests : TestBase
{
    private readonly Mock<ILunarCalendarService> _mockLunarCalendarService;
    private readonly GenerateEventOccurrencesJob _job;

    public GenerateEventOccurrencesJobTests()
    {
        // Setup mocks for dependencies not provided by TestBase's _context
        _mockLunarCalendarService = new Mock<ILunarCalendarService>();

        // Use _context (from TestBase) directly, along with other mocks
        _job = new GenerateEventOccurrencesJob(
            new Mock<ILogger<GenerateEventOccurrencesJob>>().Object, // Provide a dummy logger
            _context, // Use real in-memory context
            _mockLunarCalendarService.Object);
    }

    [Fact]
    public async Task GenerateOccurrences_ShouldGenerateNewOccurrencesForLunarEvents()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        var lunarEvent = Event.CreateLunarEvent(
            "Lunar New Year", "LNY", EventType.Anniversary,
            new LunarDate(1, 1, false, false), RepeatRule.Yearly, familyId);
        lunarEvent.ClearDomainEvents();

        await _context.Families.AddAsync(new Family { Id = familyId, Name = "Test Family", Code = "TF", CreatedBy = TestUserId.ToString() });
        await _context.Events.AddAsync(lunarEvent);
        await _context.SaveChangesAsync();

        _mockLunarCalendarService.Setup(s => s.ConvertLunarToSolar(1, 1, year, false))
            .Returns(new DateTime(year, 2, 10, 12, 0, 0));

        // Act
        await _job.GenerateOccurrences(year, null, CancellationToken.None);

        // Assert
        var generatedOccurrence = await _context.EventOccurrences.FirstOrDefaultAsync(eo => eo.EventId == lunarEvent.Id && eo.Year == year);
        generatedOccurrence.Should().NotBeNull("because an occurrence should have been generated.");
        if (generatedOccurrence != null)
        {
            generatedOccurrence.EventId.Should().Be(lunarEvent.Id);
            generatedOccurrence.Year.Should().Be(year);
            generatedOccurrence.OccurrenceDate.Should().Be(new DateTime(year, 2, 10, 5, 0, 0, DateTimeKind.Utc));
        }
    }

    [Fact]
    public async Task GenerateOccurrences_ShouldSkipExistingOccurrences()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        var lunarEvent = Event.CreateLunarEvent(
            "Lunar New Year", "LNY", EventType.Anniversary,
            new LunarDate(1, 1, false, false), RepeatRule.Yearly, familyId);
        lunarEvent.ClearDomainEvents();

        await _context.Families.AddAsync(new Family { Id = familyId, Name = "Test Family", Code = "TF", CreatedBy = TestUserId.ToString() });
        await _context.Events.AddAsync(lunarEvent);
        var existingOccurrence = EventOccurrence.Create(lunarEvent.Id, year, new DateTime(year, 2, 10));
        await _context.EventOccurrences.AddAsync(existingOccurrence);
        await _context.SaveChangesAsync();

        _mockLunarCalendarService.Setup(s => s.ConvertLunarToSolar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new DateTime(year, 2, 10));

        // Act
        await _job.GenerateOccurrences(year, null, CancellationToken.None);

        // Assert
        // Should still only have one occurrence in the database (the existing one)
        _context.EventOccurrences.Count().Should().Be(1);
    }

    [Fact]
    public async Task GenerateOccurrences_ShouldHandleBatchesAndStopWhenNoMoreEvents()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        await _context.Families.AddAsync(new Family { Id = familyId, Name = "Test Family", Code = "TF", CreatedBy = TestUserId.ToString() });

        for (int i = 0; i < 250; i++) // More than one batch
        {
            var lunarEvent = Event.CreateLunarEvent(
                $"Lunar Event {i}", $"LE{i}", EventType.Anniversary,
                new LunarDate(1, 1, false, false), RepeatRule.Yearly, familyId);
            lunarEvent.ClearDomainEvents();
            await _context.Events.AddAsync(lunarEvent);
        }
        await _context.SaveChangesAsync();

        _mockLunarCalendarService.Setup(s => s.ConvertLunarToSolar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new DateTime(year, 2, 10, 7, 0, 0));

        // Act
        await _job.GenerateOccurrences(year, null, CancellationToken.None);

        // Assert
        _context.EventOccurrences.Count().Should().Be(250);
        foreach (var occurrence in _context.EventOccurrences.ToList())
        {
            occurrence.Year.Should().Be(year);
            occurrence.OccurrenceDate.Should().Be(new DateTime(year, 2, 10, 0, 0, 0, DateTimeKind.Utc));
        }
    }

    [Fact]
    public async Task GenerateOccurrences_ShouldHandleCancellationRequest()
    {
        // Arrange
        var year = 2024;
        var familyId = Guid.NewGuid();
        await _context.Families.AddAsync(new Family { Id = familyId, Name = "Test Family", Code = "TF", CreatedBy = TestUserId.ToString() });

        for (int i = 0; i < 5; i++)
        {
            var lunarEvent = Event.CreateLunarEvent(
                $"Lunar Event {i}", $"LE{i}", EventType.Anniversary,
                new LunarDate(1, 1, false, false), RepeatRule.Yearly, familyId);
            lunarEvent.ClearDomainEvents();
            await _context.Events.AddAsync(lunarEvent);
        }
        await _context.SaveChangesAsync();

        _mockLunarCalendarService.Setup(s => s.ConvertLunarToSolar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new DateTime(year, 2, 10));

        // Create a CancellationTokenSource that will cancel after the first event
        var cts = new CancellationTokenSource();
        // Setup mock for ToListAsync to throw OperationCanceledException
        // We need to mock the IQueryable<Event> or the ToListAsync directly for this.
        // For simplicity in a unit test, we can assume the job method handles the token.

        // Act
        Func<Task> act = async () => await _job.GenerateOccurrences(year, null, cts.Token);
        cts.Cancel(); // Immediately cancel

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
