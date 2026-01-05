using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.Inputs;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Common; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using FluentAssertions;
using MediatR; // Added
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IMediator> _mediatorMock; // Added
    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _mediatorMock = new Mock<IMediator>(); // Added
        _handler = new UpdateEventCommandHandler(_context, _authorizationServiceMock.Object, _mediatorMock.Object); // Modified
    }

    [Fact]
    public async Task Handle_ShouldUpdateEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateSolarEvent("Old Name", "EVT-OLD", EventType.Other, new DateTime(2024, 1, 1), RepeatRule.None, familyId);
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "New Name",
            Description = "New Description",
            FamilyId = familyId,
            Type = EventType.Birth,
            CalendarType = CalendarType.Solar,
            SolarDate = new DateTime(2025, 1, 1),
            RepeatRule = RepeatRule.Yearly
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.FindAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.Description.Should().Be(command.Description);
        updatedEvent.Type.Should().Be(command.Type);
        updatedEvent.CalendarType.Should().Be(command.CalendarType);
        updatedEvent.SolarDate.Should().Be(command.SolarDate);
        updatedEvent.LunarDate.Should().BeNull();
        updatedEvent.RepeatRule.Should().Be(command.RepeatRule);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventUpdatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), FamilyId = Guid.NewGuid() };
        _authorizationServiceMock.Setup(x => x.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.EventNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), FamilyId = familyId };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldAddAndRemoveMembers_WhenUpdatingRelatedMemberIds()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        var member3Id = Guid.NewGuid();

        var existingEvent = Event.CreateSolarEvent("Test Event", "EVT-TEST", EventType.Other, new DateTime(2024, 5, 10), RepeatRule.None, familyId);
        existingEvent.Id = eventId;
        existingEvent.AddEventMember(member1Id);
        _context.Events.Add(existingEvent);
        _context.Members.Add(new Member("first", "last", "c1", familyId) { Id = member2Id });
        _context.Members.Add(new Member("first2", "last2", "c2", familyId) { Id = member3Id });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            FamilyId = familyId,
            Name = "Updated Event",
            CalendarType = CalendarType.Solar,
            SolarDate = new DateTime(2024, 5, 10),
            RepeatRule = RepeatRule.None,
            Type = EventType.Other,
            RelatedMemberIds = new List<Guid> { member2Id, member3Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.EventMembers.Should().HaveCount(2);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().NotContain(member1Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member2Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member3Id);
    }

    [Fact]
    public async Task Handle_ShouldRemoveAllMembers_WhenRelatedMemberIdsListIsEmpty()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        var existingEvent = Event.CreateSolarEvent("Test Event", "EVT-TEST", EventType.Other, new DateTime(2024, 5, 10), RepeatRule.None, familyId);
        existingEvent.Id = eventId;
        existingEvent.AddEventMember(member1Id);
        existingEvent.AddEventMember(member2Id);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            FamilyId = familyId,
            Name = "Updated Event",
            CalendarType = CalendarType.Solar,
            SolarDate = new DateTime(2024, 5, 10),
            RepeatRule = RepeatRule.None,
            Type = EventType.Other,
            RelatedMemberIds = new List<Guid>() // Empty list
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.EventMembers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRemoveOneMember_WhenUpdatingRelatedMemberIds()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        var member3Id = Guid.NewGuid();

        var existingEvent = Event.CreateSolarEvent("Test Event", "EVT-TEST", EventType.Other, new DateTime(2024, 5, 10), RepeatRule.None, familyId);
        existingEvent.Id = eventId;
        existingEvent.AddEventMember(member1Id);
        existingEvent.AddEventMember(member2Id);
        existingEvent.AddEventMember(member3Id);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            FamilyId = familyId,
            Name = "Updated Event",
            CalendarType = CalendarType.Solar,
            SolarDate = new DateTime(2024, 5, 10),
            RepeatRule = RepeatRule.None,
            Type = EventType.Other,
            RelatedMemberIds = new List<Guid> { member1Id, member3Id } // Remove member2Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.EventMembers.Should().HaveCount(2);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member1Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().NotContain(member2Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member3Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật sự kiện Lunar thành công khi được ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị Family, Event Lunar và thiết lập ủy quyền.
    ///    - Act: Gửi UpdateEventCommand với CalendarType là Lunar.
    ///    - Assert: Kiểm tra kết quả thành công, sự kiện được cập nhật, và các thuộc tính LunarDate chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải cập nhật đúng sự kiện Lunar khi dữ liệu hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateLunarEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateLunarEvent(
            "Old Lunar Event",
            "EVT-LUNAR-OLD",
            EventType.Other,
            new LunarDate(1, 1, false),
            RepeatRule.None,
            familyId
        );
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "New Lunar Event Name",
            Description = "New Lunar Description",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 15, Month = 8, IsLeapMonth = false },
            RepeatRule = RepeatRule.Yearly
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var updatedEvent = await _context.Events.FindAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.Description.Should().Be(command.Description);
        updatedEvent.Type.Should().Be(command.Type);
        updatedEvent.CalendarType.Should().Be(command.CalendarType);
        updatedEvent.SolarDate.Should().BeNull();
        updatedEvent.LunarDate!.Day.Should().Be(command.LunarDate!.Day);
        updatedEvent.LunarDate.Month.Should().Be(command.LunarDate.Month);
        updatedEvent.LunarDate.IsLeapMonth.Should().Be(command.LunarDate.IsLeapMonth);
        updatedEvent.RepeatRule.Should().Be(command.RepeatRule);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventUpdatedEvent)
        )), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi cập nhật sự kiện Solar nhưng cung cấp LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một sự kiện Solar hiện có.
    ///    - Act: Gửi UpdateEventCommand với SolarDate và LunarDate.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar không được có LunarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingSolarEventWithLunarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateSolarEvent(
            "Solar Event",
            "EVT-SOLAR",
            EventType.Other,
            new DateTime(2024, 1, 1),
            RepeatRule.None,
            familyId
        );
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
            FamilyId = familyId,
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false }, // Invalid for Solar
            Type = EventType.Other,
            RepeatRule = RepeatRule.None
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Solar event cannot have a LunarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi cập nhật sự kiện Lunar nhưng cung cấp SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một sự kiện Lunar hiện có.
    ///    - Act: Gửi UpdateEventCommand với LunarDate và SolarDate.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar không được có SolarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingLunarEventWithSolarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateLunarEvent(
            "Lunar Event",
            "EVT-LUNAR",
            EventType.Other,
            new LunarDate(1, 1, false),
            RepeatRule.None,
            familyId
        );
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
            FamilyId = familyId,
            CalendarType = CalendarType.Lunar,
            SolarDate = DateTime.Now, // Invalid for Lunar
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false },
            Type = EventType.Other,
            RepeatRule = RepeatRule.None
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Lunar event cannot have a SolarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi cập nhật sự kiện Solar mà không cung cấp SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một sự kiện Solar hiện có.
    ///    - Act: Gửi UpdateEventCommand với CalendarType là Solar nhưng SolarDate là null.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar yêu cầu SolarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingSolarEventWithNullSolarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateSolarEvent(
            "Solar Event",
            "EVT-SOLAR",
            EventType.Other,
            new DateTime(2024, 1, 1),
            RepeatRule.None,
            familyId
        );
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
            FamilyId = familyId,
            CalendarType = CalendarType.Solar,
            SolarDate = null, // Missing SolarDate
            Type = EventType.Other,
            RepeatRule = RepeatRule.None
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Solar event must have a SolarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi cập nhật sự kiện Lunar mà không cung cấp LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một sự kiện Lunar hiện có.
    ///    - Act: Gửi UpdateEventCommand với CalendarType là Lunar nhưng LunarDate là null.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar yêu cầu LunarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingLunarEventWithNullLunarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEvent = Event.CreateLunarEvent(
            "Lunar Event",
            "EVT-LUNAR",
            EventType.Other,
            new LunarDate(1, 1, false),
            RepeatRule.None,
            familyId
        );
        existingEvent.Id = eventId;
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
            FamilyId = familyId,
            CalendarType = CalendarType.Lunar,
            LunarDate = null, // Missing LunarDate
            Type = EventType.Other,
            RepeatRule = RepeatRule.None
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Lunar event must have a LunarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }
}
