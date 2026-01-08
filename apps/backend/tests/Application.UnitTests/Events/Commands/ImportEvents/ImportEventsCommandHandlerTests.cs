using backend.Application.Common.Constants;
using backend.Application.Common.Dtos; // Added
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.ImportEvents;
using backend.Application.Events.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore; // Added
using Moq;
using Xunit;



namespace backend.Application.UnitTests.Events.Commands.ImportEvents;

public class ImportEventsCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly ImportEventsCommandHandler _handler;

    public ImportEventsCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new ImportEventsCommandHandler(_context, _mapper, _authorizationServiceMock.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler có thể nhập các sự kiện thành công, bao gồm cả Solar và Lunar, cùng với các thành viên liên quan.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và một số thành viên. Tạo ImportEventsCommand với danh sách EventDto.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thành công, các sự kiện và thành viên liên quan được thêm vào database đúng cách.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải nhập đúng dữ liệu sự kiện và thiết lập các mối quan hệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldImportEventsSuccessfullyForSolarAndLunarEvents()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        var member1 = new Member("John", "Doe", "JD", family.Id);
        var member2 = new Member("Jane", "Doe", "JANE", family.Id);
        _context.Families.Add(family);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var eventDtos = new List<EventDto>
        {
            new EventDto
            {
                Name = "Solar Event Import",
                Code = "SEI1",
                Type = EventType.Birth,
                CalendarType = CalendarType.Solar,
                SolarDate = new DateTime(2000, 1, 1),
                RepeatRule = RepeatRule.Yearly,
                Description = "Solar Event Description",
                Color = "#FF0000",
                EventMembers = new List<EventMemberDto> { new EventMemberDto { MemberId = member1.Id } }
            },
            new EventDto
            {
                Name = "Lunar Event Import",
                Code = "LEI1",
                Type = EventType.Other,
                CalendarType = CalendarType.Lunar,
                LunarDate = new LunarDateDto { Day = 15, Month = 8, IsLeapMonth = false },
                RepeatRule = RepeatRule.None,
                Description = "Lunar Event Description",
                Color = "#00FF00",
                EventMembers = new List<EventMemberDto> { new EventMemberDto { MemberId = member2.Id } }
            }
        };

        var command = new ImportEventsCommand(family.Id, eventDtos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Events.Should().HaveCount(2);

        var importedEvent1 = _context.Events.Include(e => e.EventMembers).FirstOrDefault(e => e.Name == "Solar Event Import");
        importedEvent1.Should().NotBeNull();
        importedEvent1!.FamilyId.Should().Be(family.Id);
        importedEvent1.SolarDate.Should().Be(new DateTime(2000, 1, 1));
        importedEvent1.EventMembers.Should().HaveCount(1);
        importedEvent1.EventMembers.First().MemberId.Should().Be(member1.Id);

        var importedEvent2 = _context.Events.Include(e => e.EventMembers).FirstOrDefault(e => e.Name == "Lunar Event Import");
        importedEvent2.Should().NotBeNull();
        importedEvent2!.FamilyId.Should().Be(family.Id);
        importedEvent2.LunarDate!.Day.Should().Be(15);
        importedEvent2.LunarDate.Month.Should().Be(8);
        importedEvent2.EventMembers.Should().HaveCount(1);
        importedEvent2.EventMembers.First().MemberId.Should().Be(member2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi familyId không tồn tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo ImportEventsCommand với familyId không tồn tại.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện phải thuộc về một family tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenFamilyNotFound()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(nonExistentFamilyId)).Returns(true);

        var command = new ImportEventsCommand(nonExistentFamilyId, new List<EventDto>
        {
            new EventDto { Name = "Event 1", Type = EventType.Other, CalendarType = CalendarType.Solar, SolarDate = DateTime.Now }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {nonExistentFamilyId} not found.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi người dùng không được ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và thiết lập ủy quyền trả về false.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi AccessDenied.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng phải có quyền quản lý family để nhập sự kiện.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserNotAuthorized()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Unauthorized Family", Code = "UF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(false);

        var command = new ImportEventsCommand(family.Id, new List<EventDto>
        {
            new EventDto { Name = "Event 1", Type = EventType.Other, CalendarType = CalendarType.Solar, SolarDate = DateTime.Now }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi sự kiện Solar không có SolarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và lệnh nhập với Solar event nhưng SolarDate là null.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Solar yêu cầu SolarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenSolarEventHasNoSolarDate()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var command = new ImportEventsCommand(family.Id, new List<EventDto>
        {
            new EventDto { Name = "Invalid Solar Event", Type = EventType.Other, CalendarType = CalendarType.Solar, SolarDate = null }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Solar event 'Invalid Solar Event' must have a SolarDate.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi sự kiện Lunar không có LunarDate.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và lệnh nhập với Lunar event nhưng LunarDate là null.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Sự kiện Lunar yêu cầu LunarDate.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenLunarEventHasNoLunarDate()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var command = new ImportEventsCommand(family.Id, new List<EventDto>
        {
            new EventDto { Name = "Invalid Lunar Event", Type = EventType.Other, CalendarType = CalendarType.Lunar, LunarDate = null }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Lunar event 'Invalid Lunar Event' must have LunarDate details.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi có thành viên liên quan không tồn tại trong family.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và lệnh nhập với event có related member ID không tồn tại.
    ///    - Act: Gửi ImportEventsCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Tất cả related members phải tồn tại trong family.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenRelatedMemberNotFoundInFamily()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var nonExistentMemberId = Guid.NewGuid();

        var command = new ImportEventsCommand(family.Id, new List<EventDto>
        {
            new EventDto
            {
                Name = "Event with Invalid Member",
                Type = EventType.Other,
                CalendarType = CalendarType.Solar,
                SolarDate = DateTime.Now,
                EventMembers = new List<EventMemberDto> { new EventMemberDto { MemberId = nonExistentMemberId } }
            }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"One or more associated members not found in Family {family.Id}: {nonExistentMemberId}");
    }
}
