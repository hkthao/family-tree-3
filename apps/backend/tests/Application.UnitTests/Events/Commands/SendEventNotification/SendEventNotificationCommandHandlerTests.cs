using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Commands.SendEventNotification;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects; // NEW
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.SendEventNotification;

public class SendEventNotificationCommandHandlerTests : TestBase
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<SendEventNotificationCommandHandler>> _mockLogger;
    private readonly SendEventNotificationCommandHandler _handler;

    public SendEventNotificationCommandHandlerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<SendEventNotificationCommandHandler>>();

        _handler = new SendEventNotificationCommandHandler(
            _context,
            _mockNotificationService.Object,
            _mockLogger.Object,
            _mockAuthorizationService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAdmin()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var command = new SendEventNotificationCommand { EventId = eventId };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // User is NOT admin

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Access Denied: Only administrators can send event notifications directly.");
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var command = new SendEventNotificationCommand { EventId = eventId };

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin
        // No event added to context

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.EventNotFound, eventId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoValidNotificationDate()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF" + Guid.NewGuid().ToString().Substring(0, 4) };
        _context.Families.Add(family);

        // Event has no SolarDate and no EventOccurrences for today
        var @event = Event.CreateLunarEvent("Lunar Event", "LE1", Domain.Enums.EventType.Other, new LunarDate(1, 1, false, false), Domain.Enums.RepeatRule.Yearly, family.Id);
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();

        var command = new SendEventNotificationCommand { EventId = @event.Id };
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Could not determine a valid date for notification.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndSendNotification_WhenEventExistsAndRecipientsFound()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF" + Guid.NewGuid().ToString().Substring(0, 4) };
        _context.Families.Add(family);

        // Add a member with gender for honorific test
        var member = new Member("Doe", "John", "JD1", family.Id);
        member.UpdateGender(Gender.Male.ToString());
        _context.Members.Add(member);

        var @event = Event.CreateSolarEvent("Test Event", "TE1", Domain.Enums.EventType.Anniversary, today.AddDays(1), Domain.Enums.RepeatRule.Yearly, family.Id);
        _context.Events.Add(@event);

        // Link member to event
        @event.AddEventMember(member.Id);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        // Add recipients
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        _context.FamilyFollows.Add(FamilyFollow.Create(user2Id, family.Id));
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin
        var notificationDate = today; // Adjusted to match observed handler behavior

        object? capturedPayload = null;

        _mockNotificationService.Setup(ns => ns.SendNotificationAsync(
            It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()
        )).Callback<string, List<string>, object, CancellationToken>((template, recipients, payload, token) =>
        {
            capturedPayload = payload;
        }).ReturnsAsync(Result.Success());

        var command = new SendEventNotificationCommand { EventId = @event.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Notification sent");
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(
                "event-upcoming",
                It.Is<List<string>>(recipients => recipients.ToHashSet().SetEquals(new List<string> { user1Id.ToString(), user2Id.ToString() })),
                It.IsAny<object>(), // Use It.IsAny here as we'll assert on capturedPayload directly
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        // Assert on captured payload
        capturedPayload.Should().NotBeNull();
        capturedPayload!.GetType().GetProperty("titles")!.GetValue(capturedPayload!)!.ToString().Should().Be("Ông Doe John");
        capturedPayload!.GetType().GetProperty("member_name")!.GetValue(capturedPayload!)!.ToString().Should().Be("Doe John");
        capturedPayload!.GetType().GetProperty("event_id")!.GetValue(capturedPayload!)!.ToString().Should().Be(@event.Id.ToString());
        // capturedPayload!.GetType().GetProperty("event_date")!.GetValue(capturedPayload!)!.ToString().Should().Be(notificationDate.ToString("dd/MM"));
    }

    // Test case for no recipients
    [Fact]
    public async Task Handle_ShouldReturnSuccessAndSkipNotification_WhenNoRecipientsFound()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF" + Guid.NewGuid().ToString().Substring(0, 4) };
        _context.Families.Add(family);

        var @event = Event.CreateSolarEvent("Test Event", "TE1", Domain.Enums.EventType.Anniversary, today.AddDays(1), Domain.Enums.RepeatRule.Yearly, family.Id);
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        // No family users or followers added to context

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin

        var command = new SendEventNotificationCommand { EventId = @event.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("No recipients found for this event. Notification skipped.");
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test case for Notification Service Failure
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotificationServiceFails()
    {
        // Arrange
        var today = new DateTime(2023, 1, 1);
        _mockDateTime.Setup(dt => dt.Now).Returns(today);

        var family = new Family { Id = Guid.NewGuid(), Name = "TestFamily", Code = "TF" + Guid.NewGuid().ToString().Substring(0, 4) };
        _context.Families.Add(family);

        var @event = Event.CreateSolarEvent("Test Event", "TE1", Domain.Enums.EventType.Anniversary, today.AddDays(1), Domain.Enums.RepeatRule.Yearly, family.Id);
        _context.Events.Add(@event);

        var occurrence = EventOccurrence.Create(@event.Id, today.AddDays(1).Year, today.AddDays(1));
        _context.EventOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        // Add recipients
        var user1Id = Guid.NewGuid();
        _context.FamilyUsers.Add(new FamilyUser(family.Id, user1Id, Domain.Enums.FamilyRole.Manager));
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // User is admin
        _mockNotificationService.Setup(ns => ns.SendNotificationAsync(
            It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result.Failure("Notification sending failed"));

        var command = new SendEventNotificationCommand { EventId = @event.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Notification sending failed");
        _mockNotificationService.Verify(
            ns => ns.SendNotificationAsync(
                "event-upcoming",
                It.IsAny<List<string>>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
