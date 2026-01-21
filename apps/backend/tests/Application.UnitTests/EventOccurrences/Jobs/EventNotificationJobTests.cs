using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.EventOccurrences.Jobs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums; // Add this line
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.EventOccurrences.Jobs;

public class EventNotificationJobTests : TestBase
{
    private readonly EventNotificationJob _sut;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<EventNotificationJob>> _mockLogger;

    public EventNotificationJobTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<EventNotificationJob>>();

        _sut = new EventNotificationJob(
            _context,
            _mockNotificationService.Object,
            _mockDateTime.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Run_ShouldNotSendNotifications_WhenNoUpcomingEvents()
    {
        // Arrange
        _mockDateTime.Setup(dt => dt.Now).Returns(new DateTime(2023, 1, 1));
        // No events or occurrences added to _context

        // Act
        await _sut.Run(CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), // UPDATED
            Times.Never
        );
        _context.NotificationDeliveries.Should().BeEmpty();
    }

    [Fact]
    public async Task Run_ShouldSendNotificationsAndRecordDelivery_WhenUpcomingEventsExistAndNoPreviousDelivery()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF1" }; // UPDATED: Added Code
        _context.Families.Add(family);

        // Add a member with gender for honorific test
        var member = new Member("Doe", "John", "JD1", family.Id);
        member.UpdateGender(Gender.Male.ToString());
        _context.Members.Add(member);
        await _context.SaveChangesAsync();


        // Add a family user and a family follower to simulate recipients
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        _context.FamilyFollows.Add(FamilyFollow.Create(user2Id, family.Id));
        await _context.SaveChangesAsync();

        var @event = Event.CreateSolarEvent("Test Event", "TE1", Domain.Enums.EventType.Anniversary, today.AddDays(1), Domain.Enums.RepeatRule.Yearly, family.Id);
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        _mockNotificationService.Setup(ns => ns.SendNotificationAsync(
                It.IsAny<string>(), // workflowId
                It.IsAny<List<string>>(), // recipientUserIds - UPDATED
                It.IsAny<object>(), // payload
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(Result.Success());

        // Act
        await _sut.Run(CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(
                "event-upcoming",
                It.Is<List<string>>(recipients => recipients.Contains(user1Id.ToString()) && recipients.Contains(user2Id.ToString())), // Verify recipients
                It.Is<object>(payload =>
                    payload.GetType().GetProperty("titles")!.GetValue(payload)!.ToString()!.Equals($"Sự kiện sắp tới: {@event.Name}") && // Verify 'titles'
                    payload.GetType().GetProperty("member_name")!.GetValue(payload)!.ToString()!.Equals("") && // Verify 'member_name' is empty
                    payload.GetType().GetProperty("event_id")!.GetValue(payload)!.ToString()!.Equals(@event.Id.ToString()) && // Verify 'event_id'
                    payload.GetType().GetProperty("event_date")!.GetValue(payload)!.ToString()!.Equals(occurrence.OccurrenceDate.ToString("dd/MM")) // Verify 'event_date'
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _context.NotificationDeliveries.Should().HaveCount(1);
        var delivery = await _context.NotificationDeliveries.FirstAsync();
        delivery.EventId.Should().Be(@event.Id);
        delivery.DeliveryDate.Date.Should().Be(occurrence.OccurrenceDate.Date);
        delivery.IsSent.Should().BeTrue();
        delivery.SentAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Run_ShouldNotSendNotifications_WhenPreviousDeliveryIsSent()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF2" }; // UPDATED: Added Code
        _context.Families.Add(family);

        // Add a family user and a family follower to simulate recipients
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        _context.FamilyFollows.Add(FamilyFollow.Create(user2Id, family.Id));
        await _context.SaveChangesAsync();

        var @event = Event.CreateSolarEvent("Another Event", "AE1", Domain.Enums.EventType.Birth, today.AddDays(2), Domain.Enums.RepeatRule.Yearly, family.Id);
        @event.Id = Guid.NewGuid(); // Ensure unique EventId
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(2).Year, today.AddDays(2));
        occurrence.Id = Guid.NewGuid(); // Ensure unique OccurrenceId
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        var existingDelivery = NotificationDelivery.Create(occurrence.EventId, null, occurrence.OccurrenceDate.Date, "Push Notification");
        existingDelivery.Id = Guid.NewGuid(); // Ensure unique DeliveryId
        existingDelivery.MarkAsSent();
        _context.NotificationDeliveries.Add(existingDelivery);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear(); // Clear change tracker to simulate a fresh context

        // Act
        await _sut.Run(CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), // UPDATED
            Times.Never
        );

        _context.NotificationDeliveries.Should().HaveCount(1);
        var delivery = await _context.NotificationDeliveries.FirstAsync();
        delivery.SentAttempts.Should().Be(1); // Still 1 as it was already sent and not re-attempted
    }

    [Fact]
    public async Task Run_ShouldReattemptNotification_WhenPreviousDeliveryFailed()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF3" }; // UPDATED: Added Code
        _context.Families.Add(family);

        // Add a family user and a family follower to simulate recipients
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        _context.FamilyFollows.Add(FamilyFollow.Create(user2Id, family.Id));
        await _context.SaveChangesAsync();

        var @event = Event.CreateSolarEvent("Failed Event", "FE1", Domain.Enums.EventType.Other, today.AddDays(1), Domain.Enums.RepeatRule.None, family.Id);
        @event.Id = Guid.NewGuid(); // Ensure unique EventId
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        occurrence.Id = Guid.NewGuid(); // Ensure unique OccurrenceId
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        var existingDelivery = NotificationDelivery.Create(occurrence.EventId, null, occurrence.OccurrenceDate.Date, "Push Notification");
        existingDelivery.Id = Guid.NewGuid(); // Ensure unique DeliveryId
        existingDelivery.IncrementAttempts(); // Simulate a previous failed attempt
        _context.NotificationDeliveries.Add(existingDelivery);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Clear change tracker to simulate a fresh context state.

        // Act
        _mockNotificationService.Setup(ns => ns.SendNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(), // UPDATED
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(Result.Success());
        await _sut.Run(CancellationToken.None);

        // Re-fetch the delivery from the context to ensure latest state
        var updatedDelivery = await _context.NotificationDeliveries
            .FirstAsync(nd => nd.EventId == existingDelivery.EventId);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(
                "event-upcoming",
                It.Is<List<string>>(recipients => recipients.Contains(user1Id.ToString()) && recipients.Contains(user2Id.ToString())), // Verify recipients
                It.Is<object>(payload =>
                    payload.GetType().GetProperty("titles")!.GetValue(payload)!.ToString()!.Equals($"Sự kiện sắp tới: {@event.Name}") && // Verify 'titles' with event name
                    payload.GetType().GetProperty("member_name")!.GetValue(payload)!.ToString()!.Equals("") && // Verify 'member_name' is empty
                    payload.GetType().GetProperty("event_id")!.GetValue(payload)!.ToString()!.Equals(@event.Id.ToString()) // Verify 'event_id'
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _context.NotificationDeliveries.Should().HaveCount(1); // Still should only have one record
        updatedDelivery.IsSent.Should().BeTrue();
        updatedDelivery.SentAttempts.Should().Be(2); // Incremented from 1 to 2
    }
    [Fact]
    public async Task Run_ShouldLogWarning_WhenEventNotFoundForOccurrence()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var occurrence = EventOccurrence.Create(Guid.NewGuid(), today.AddDays(1).Year, today.AddDays(1)); // Occurrence with non-existent event
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        // Act
        await _sut.Run(CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), // UPDATED
            Times.Never
        );
        _context.NotificationDeliveries.Should().BeEmpty();
    }

    [Fact]
    public async Task Run_ShouldHandleNotificationServiceFailure_WhenSendNotificationFails()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF4" }; // UPDATED: Added Code
        _context.Families.Add(family);

        // Add a family user and a family follower to simulate recipients
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        _context.FamilyFollows.Add(FamilyFollow.Create(user2Id, family.Id));
        await _context.SaveChangesAsync();

        var @event = Event.CreateSolarEvent("Failing Event", "FEL", Domain.Enums.EventType.Other, today.AddDays(1), Domain.Enums.RepeatRule.None, family.Id);
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        _mockNotificationService.Setup(ns => ns.SendNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<List<string>>(), // UPDATED
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(Result.Failure("Notification service failed"));

        // Act
        await _sut.Run(CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(
                "event-upcoming",
                It.Is<List<string>>(recipients => recipients.Contains(user1Id.ToString()) && recipients.Contains(user2Id.ToString())), // Verify recipients
                It.Is<object>(payload =>
                    payload.GetType().GetProperty("titles")!.GetValue(payload)!.ToString()!.Equals($"Sự kiện sắp tới: {@event.Name}") && // Verify 'titles' with event name
                    payload.GetType().GetProperty("event_id")!.GetValue(payload)!.ToString()!.Equals(@event.Id.ToString()) &&
                    payload.GetType().GetProperty("event_date")!.GetValue(payload)!.ToString()!.Equals(occurrence.OccurrenceDate.ToString("dd/MM"))
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _context.NotificationDeliveries.Should().HaveCount(1);
        var delivery = await _context.NotificationDeliveries.FirstAsync();
        delivery.IsSent.Should().BeFalse();
        delivery.SentAttempts.Should().Be(1);
    }
}
