using System;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests.Entities;

public class EventOccurrenceTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var year = 2024;
        var occurrenceDate = new DateTime(2024, 1, 15);

        // Act
        var eventOccurrence = EventOccurrence.Create(eventId, year, occurrenceDate);

        // Assert
        eventOccurrence.Should().NotBeNull();
        eventOccurrence.EventId.Should().Be(eventId);
        eventOccurrence.Year.Should().Be(year);
        eventOccurrence.OccurrenceDate.Should().Be(occurrenceDate);
        eventOccurrence.Id.Should().NotBeEmpty(); // BaseAuditableEntity should set Id
        eventOccurrence.Created.Should().NotBe(default(DateTime)); // BaseAuditableEntity should set Created
    }
}
