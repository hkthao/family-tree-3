using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Services;

public class FamilyTreeServiceTests : TestBase
{
    private readonly FamilyTreeService _familyTreeService;

    public FamilyTreeServiceTests()
    {
        // No IStringLocalizer needed anymore, as it was removed from FamilyTreeService
        _familyTreeService = new FamilyTreeService(_context);
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNewLunarDeathDate_CreatesNewLunarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        DateOnly? dateOfDeath = null; // No solar death date
        var lunarDateOfDeath = new LunarDate(15, 5, false, false); // New lunar death date

        // Add member to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var lunarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Lunar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(lunarDeathEvent);
        Assert.Equal(EventType.Death, lunarDeathEvent.Type);
        Assert.Equal(CalendarType.Lunar, lunarDeathEvent.CalendarType);
        Assert.NotNull(lunarDeathEvent.LunarDate);
        Assert.Equal(lunarDateOfDeath.Day, lunarDeathEvent.LunarDate.Day);
        Assert.Equal(lunarDateOfDeath.Month, lunarDeathEvent.LunarDate.Month);
        Assert.Equal(lunarDateOfDeath.IsLeapMonth, lunarDeathEvent.LunarDate.IsLeapMonth);
        Assert.Contains(lunarDeathEvent.EventMembers, em => em.MemberId == memberId);
        Assert.Equal("Ngày giỗ của " + memberFullName, lunarDeathEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenUpdatedLunarDeathDate_UpdatesExistingLunarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        DateOnly? dateOfDeath = null;
        var initialLunarDateOfDeath = new LunarDate(1, 1, false, false);
        var updatedLunarDateOfDeath = new LunarDate(20, 10, false, true);

        // Add member and initial lunar death event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateLunarEvent(
            "Ngày giỗ của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Death,
            initialLunarDateOfDeath,
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, updatedLunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var updatedLunarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Lunar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(updatedLunarDeathEvent);
        Assert.Equal(initialEvent.Id, updatedLunarDeathEvent.Id); // Ensure it's the same event updated
        Assert.NotNull(updatedLunarDeathEvent.LunarDate);
        Assert.Equal(updatedLunarDateOfDeath.Day, updatedLunarDeathEvent.LunarDate.Day);
        Assert.Equal(updatedLunarDateOfDeath.Month, updatedLunarDeathEvent.LunarDate.Month);
        Assert.Equal(updatedLunarDateOfDeath.IsLeapMonth, updatedLunarDeathEvent.LunarDate.IsLeapMonth);
        Assert.Equal("Ngày giỗ của " + memberFullName, updatedLunarDeathEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNullLunarDeathDate_RemovesExistingLunarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        DateOnly? dateOfDeath = null;
        var initialLunarDateOfDeath = new LunarDate(1, 1, false, false);
        LunarDate? lunarDateOfDeath = null; // Clearing lunar death date

        // Add member and initial lunar death event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateLunarEvent(
            "Ngày giỗ của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Death,
            initialLunarDateOfDeath,
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var removedLunarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Lunar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.Null(removedLunarDeathEvent);
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNewSolarDeathDate_CreatesNewSolarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var dateOfDeath = new DateOnly(2023, 1, 15); // New solar death date
        LunarDate? lunarDateOfDeath = null; // No lunar death date

        // Add member to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var solarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Solar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(solarDeathEvent);
        Assert.Equal(EventType.Death, solarDeathEvent.Type);
        Assert.Equal(CalendarType.Solar, solarDeathEvent.CalendarType);
        Assert.Equal(dateOfDeath.ToDateTime(TimeOnly.MinValue), solarDeathEvent.SolarDate);
        Assert.Contains(solarDeathEvent.EventMembers, em => em.MemberId == memberId);
        Assert.Equal("Ngày mất của " + memberFullName, solarDeathEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenUpdatedSolarDeathDate_UpdatesExistingSolarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var initialDateOfDeath = new DateOnly(2020, 1, 1);
        var updatedDateOfDeath = new DateOnly(2023, 10, 20);
        LunarDate? lunarDateOfDeath = null;

        // Add member and initial solar death event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateSolarEvent(
            "Ngày mất của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Death,
            initialDateOfDeath.ToDateTime(TimeOnly.MinValue),
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, updatedDateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var updatedSolarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Solar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(updatedSolarDeathEvent);
        Assert.Equal(initialEvent.Id, updatedSolarDeathEvent.Id); // Ensure it's the same event updated
        Assert.Equal(updatedDateOfDeath.ToDateTime(TimeOnly.MinValue), updatedSolarDeathEvent.SolarDate);
        Assert.Equal("Ngày mất của " + memberFullName, updatedSolarDeathEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNullSolarDeathDate_RemovesExistingSolarDeathEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        DateOnly? dateOfDeath = null; // Clearing solar death date
        LunarDate? lunarDateOfDeath = null;

        // Add member and initial solar death event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateSolarEvent(
            "Ngày mất của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Death,
            new DateOnly(2020, 1, 1).ToDateTime(TimeOnly.MinValue),
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var removedSolarDeathEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Solar && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.Null(removedSolarDeathEvent);
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNewBirthDate_CreatesNewBirthEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var dateOfBirth = new DateOnly(1990, 1, 1); // New birth date
        DateOnly? dateOfDeath = null;
        LunarDate? lunarDateOfDeath = null;

        // Add member to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var birthEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(birthEvent);
        Assert.Equal(EventType.Birth, birthEvent.Type);
        Assert.Equal(CalendarType.Solar, birthEvent.CalendarType);
        Assert.Equal(dateOfBirth.ToDateTime(TimeOnly.MinValue), birthEvent.SolarDate);
        Assert.Contains(birthEvent.EventMembers, em => em.MemberId == memberId);
        Assert.Equal("Ngày sinh của " + memberFullName, birthEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenUpdatedBirthDate_UpdatesExistingBirthEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        var initialDateOfBirth = new DateOnly(1980, 5, 10);
        var updatedDateOfBirth = new DateOnly(1990, 1, 1);
        DateOnly? dateOfDeath = null;
        LunarDate? lunarDateOfDeath = null;

        // Add member and initial birth event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateSolarEvent(
            "Ngày sinh của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Birth,
            initialDateOfBirth.ToDateTime(TimeOnly.MinValue),
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, updatedDateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var updatedBirthEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.NotNull(updatedBirthEvent);
        Assert.Equal(initialEvent.Id, updatedBirthEvent.Id); // Ensure it's the same event updated
        Assert.Equal(updatedDateOfBirth.ToDateTime(TimeOnly.MinValue), updatedBirthEvent.SolarDate);
        Assert.Equal("Ngày sinh của " + memberFullName, updatedBirthEvent.Name); // Check hardcoded name
    }

    [Fact]
    public async Task SyncMemberLifeEvents_GivenNullBirthDate_RemovesExistingBirthEvent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var memberFullName = "Test Member";
        DateOnly? dateOfBirth = null; // Clearing birth date
        DateOnly? dateOfDeath = null;
        LunarDate? lunarDateOfDeath = null;

        // Add member and initial birth event to context
        var member = CreateMember("Test", "Member", "M1", familyId, isDeceased: false, id: memberId);

        await _context.Members.AddAsync(member);

        var initialEvent = Event.CreateSolarEvent(
            "Ngày sinh của " + memberFullName, // Hardcoded name
            $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
            EventType.Birth,
            new DateOnly(1990, 1, 1).ToDateTime(TimeOnly.MinValue),
            RepeatRule.Yearly,
            familyId
        );
        initialEvent.AddEventMember(memberId);
        await _context.Events.AddAsync(initialEvent);
        await _context.SaveChangesAsync();

        // Act
        await _familyTreeService.SyncMemberLifeEvents(memberId, familyId, dateOfBirth, dateOfDeath, lunarDateOfDeath, memberFullName, CancellationToken.None);

        // Assert
        var removedBirthEvent = await _context.Events.FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == memberId));

        Assert.Null(removedBirthEvent);
    }
}
