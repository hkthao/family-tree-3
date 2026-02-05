using backend.Application.Common.Constants; // Added
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Services; // Added for ILunarCalendarService
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.Inputs;
using backend.Application.UnitTests.Common;
using backend.Domain.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR; // Added
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<ILunarCalendarService> _lunarCalendarServiceMock; // Added
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _lunarCalendarServiceMock = new Mock<ILunarCalendarService>(); // Added
        _handler = new CreateEventCommandHandler(_context, _authorizationServiceMock.Object, _lunarCalendarServiceMock.Object); // Modified
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "New Year Party",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Solar,
            SolarDate = new DateTime(2025, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(command.Name);
        createdEvent.Code.Should().NotBeNullOrEmpty();
        createdEvent.Type.Should().Be(command.Type);
        createdEvent.FamilyId.Should().Be(command.FamilyId);
        createdEvent.CalendarType.Should().Be(command.CalendarType);
        createdEvent.SolarDate.Should().Be(command.SolarDate);
        createdEvent.LunarDate.Should().BeNull();
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventCreatedEvent)
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new CreateEventCommand { FamilyId = familyId, Name = "Unauthorized Event" };

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Event without code",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.Code.Should().NotBeNullOrEmpty();
        createdEvent.Code.Should().StartWith("EVT-");
    }

    [Fact]
    public async Task Handle_ShouldAddRelatedMemberIds_WhenProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        _context.Members.Add(new Member("first", "last", "c1", familyId) { Id = member1Id });
        _context.Members.Add(new Member("first2", "last2", "c2", familyId) { Id = member2Id });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Event with members",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            EventMemberIds = new List<Guid> { member1Id, member2Id }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.EventMembers.Should().HaveCount(2);
        createdEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member1Id);
        createdEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member2Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo sự kiện Lunar thành công khi được ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị Family và thiết lập ủy quyền.
    ///    - Act: Gửi CreateEventCommand với CalendarType là Lunar.
    ///    - Assert: Kiểm tra kết quả thành công, sự kiện được tạo, và các thuộc tính LunarDate chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải tạo đúng sự kiện Lunar khi dữ liệu hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateLunarEventAndReturnSuccess_WhenAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF2" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Mid-Autumn Festival",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Lunar,
            LunarDate = new LunarDateInput { Day = 15, Month = 8, IsLeapMonth = false }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(command.Name);
        createdEvent.Code.Should().NotBeNullOrEmpty();
        createdEvent.Type.Should().Be(command.Type);
        createdEvent.FamilyId.Should().Be(command.FamilyId);
        createdEvent.CalendarType.Should().Be(command.CalendarType);
        createdEvent.SolarDate.Should().BeNull();
        createdEvent.LunarDate!.Day.Should().Be(command.LunarDate!.Day);
        createdEvent.LunarDate.Month.Should().Be(command.LunarDate.Month);
        createdEvent.LunarDate.IsLeapMonth.Should().Be(command.LunarDate.IsLeapMonth);
        _mockDomainEventDispatcher.Verify(d => d.DispatchEvents(It.Is<List<BaseEvent>>(events =>
            events.Any(e => e is Domain.Events.Events.EventCreatedEvent)
        )), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar không có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị CreateEventCommand với CalendarType là Solar nhưng SolarDate là null.
    ///    - Act: Gửi lệnh đến handler.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar yêu cầu SolarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSolarEventHasNoSolarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF3" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Invalid Solar Event",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Solar,
            SolarDate = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Solar event must have a SolarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Solar có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị CreateEventCommand với CalendarType là Solar và có LunarDate.
    ///    - Act: Gửi lệnh đến handler.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar không được có LunarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSolarEventHasLunarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF4" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Invalid Solar Event",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Solar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Solar event cannot have a LunarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi sự kiện Lunar không có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị CreateEventCommand với CalendarType là Lunar nhưng LunarDate là null.
    ///    - Act: Gửi lệnh đến handler.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar yêu cầu LunarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLunarEventHasNoLunarDate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF5" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Invalid Lunar Event",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Lunar,
            LunarDate = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Lunar event must have a LunarDate.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// 🎯 Mục tiêu của test: Xác minh rằng sự kiện Lunar có thể có SolarDate sau khi logic kiểm tra bị xóa.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị CreateEventCommand với CalendarType là Lunar và có SolarDate.
    ///    - Act: Gửi lệnh đến handler.
    ///    - Assert: Kiểm tra kết quả thành công.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Logic ngăn chặn Lunar event có SolarDate đã bị xóa.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSucceed_WhenLunarEventHasSolarDate_AfterLogicRemoval()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF6" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Valid Lunar Event with SolarDate",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Lunar,
            SolarDate = DateTime.Now,
            LunarDate = new LunarDateInput { Day = 1, Month = 1, IsLeapMonth = false }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh lỗi khi CalendarType không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị CreateEventCommand với CalendarType không hợp lệ.
    ///    - Act: Gửi lệnh đến handler.
    ///    - Assert: Kiểm tra kết quả thất bại và thông báo lỗi tương ứng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: CalendarType phải là Solar hoặc Lunar.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidCalendarType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF7" });
        await _context.SaveChangesAsync();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var command = new CreateEventCommand
        {
            Name = "Invalid Calendar Type Event",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = (CalendarType)99, // Invalid enum value
            SolarDate = DateTime.Now
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid CalendarType.");
        result.ErrorSource.Should().Be(ErrorSources.BadRequest);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng EventOccurrences được tạo cho sự kiện Lunar lặp lại hàng năm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị Family, ủy quyền, và một sự kiện Lunar lặp lại hàng năm. Mock ILunarCalendarService để trả về ngày dương lịch cụ thể.
    ///    - Act: Gửi CreateEventCommand.
    ///    - Assert: Kiểm tra kết quả thành công và một EventOccurrence được tạo trong cơ sở dữ liệu cho năm hiện tại.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar lặp lại hàng năm phải tự động tạo EventOccurrence cho năm hiện tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateEventOccurrence_WhenYearlyRepeatingLunarEventCreated()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF8" });
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var lunarDateInput = new LunarDateInput { Day = 15, Month = 8, IsLeapMonth = false };
        var currentYear = DateTime.Now.Year;
        var expectedSolarDate = new DateTime(currentYear, 9, 29, 0, 0, 0, DateTimeKind.Utc); // Example date

        _lunarCalendarServiceMock
            .Setup(x => x.ConvertLunarToSolar(lunarDateInput.Day, lunarDateInput.Month, currentYear, lunarDateInput.IsLeapMonth))
            .Returns(expectedSolarDate);

        var command = new CreateEventCommand
        {
            Name = "Yearly Lunar Event",
            FamilyId = familyId,
            Type = EventType.Other,
            CalendarType = CalendarType.Lunar,
            LunarDate = lunarDateInput,
            RepeatRule = RepeatRule.Yearly
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdEvent = await _context.Events.FindAsync(result.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdEvent.Should().NotBeNull();

        var eventOccurrence = await _context.EventOccurrences
            .FirstOrDefaultAsync(eo => eo.EventId == createdEvent!.Id && eo.Year == currentYear);

        eventOccurrence.Should().NotBeNull();
        eventOccurrence!.OccurrenceDate.Should().Be(expectedSolarDate);
    }
}

