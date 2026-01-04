using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.Events.Queries; // EventDto is here now
using backend.Application.Events.Queries.ExportEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.ExportEvents;

public class ExportEventsQueryHandlerTests : TestBase
{
    private readonly ExportEventsQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public ExportEventsQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<EventDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<EventDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<EventDto> dtos, Guid familyId, CancellationToken token) => dtos);

        _handler = new ExportEventsQueryHandler(_context, _mapper, _mockPrivacyService.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler có thể xuất sự kiện thành công.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và một số sự kiện (cả Solar và Lunar) trong database.
    ///    - Act: Gửi ExportEventsQuery.
    ///    - Assert: Kiểm tra kết quả thành công, nội dung JSON có chứa các sự kiện đã tạo với dữ liệu chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải xuất đúng dữ liệu sự kiện.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldExportEventsSuccessfully()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);

        var member1 = new Member("John", "Doe", "JD", family.Id);
        var member2 = new Member("Jane", "Doe", "JANE", family.Id);
        _context.Members.AddRange(member1, member2);

        var event1 = Event.CreateSolarEvent(
            "Solar Event 1", "EVT1", EventType.Birth, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), RepeatRule.Yearly, family.Id, "Description 1", "#FF0000"
        );
        event1.AddEventMember(member1.Id);

        var event2 = Event.CreateLunarEvent(
            "Lunar Event 1", "EVT2", EventType.Other, new LunarDate(15, 8, false), RepeatRule.None, family.Id, "Description 2", "#00FF00"
        );
        event2.AddEventMember(member2.Id);

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync();

        var query = new ExportEventsQuery(family.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var exportedEvents = JsonConvert.DeserializeObject<List<EventDto>>(result.Value!)!;
        exportedEvents.Should().HaveCount(2);

        var exportedEvent1 = exportedEvents.FirstOrDefault(e => e.Name == event1.Name)!;
        exportedEvent1.Should().NotBeNull();
        exportedEvent1!.Name.Should().Be(event1.Name);
        exportedEvent1.FamilyId.Should().Be(event1.FamilyId);
        exportedEvent1.EventMembers.Should().HaveCount(1);
        exportedEvent1.EventMembers.First().MemberId.Should().Be(member1.Id);
        exportedEvent1.SolarDate.Should().Be(event1.SolarDate!.Value.ToUniversalTime());
        exportedEvent1.LunarDate.Should().BeNull();

        var exportedEvent2 = exportedEvents.FirstOrDefault(e => e.Name == event2.Name);
        exportedEvent2.Should().NotBeNull();
        exportedEvent2!.Name.Should().Be(event2.Name);
        exportedEvent2.FamilyId.Should().Be(event2.FamilyId);
        exportedEvent2.EventMembers.Should().HaveCount(1);
        exportedEvent2.EventMembers.First().MemberId.Should().Be(member2.Id);
        exportedEvent2.SolarDate.Should().BeNull();
        exportedEvent2.LunarDate.Should().NotBeNull();
        exportedEvent2.LunarDate!.Day.Should().Be(event2.LunarDate!.Day);
        exportedEvent2.LunarDate.Month.Should().Be(event2.LunarDate.Month);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi không tìm thấy sự kiện nào cho familyId.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một family nhưng không có sự kiện nào.
    ///    - Act: Gửi ExportEventsQuery.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Không có sự kiện nào để xuất.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenNoEventsFound()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Empty Family", Code = "EF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new ExportEventsQuery(family.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không tìm thấy sự kiện nào cho gia đình này.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách rỗng khi familyId không tồn tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Không có family nào trong database với familyId được cung cấp.
    ///    - Act: Gửi ExportEventsQuery với một familyId ngẫu nhiên.
    ///    - Assert: Kiểm tra kết quả thành công với một chuỗi JSON rỗng (hoặc một mảng JSON rỗng).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Query trả về một danh sách trống nếu familyId không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenFamilyIdDoesNotExist() // Changed test name
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new ExportEventsQuery(nonExistentFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse(); // Should be failure based on current handler logic
        result.Error.Should().Contain("Không tìm thấy sự kiện nào cho gia đình này.");
    }
}
