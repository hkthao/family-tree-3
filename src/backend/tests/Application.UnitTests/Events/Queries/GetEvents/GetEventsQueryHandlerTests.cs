using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests : TestBase
{
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _handler = new GetEventsQueryHandler(
            _context,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách sự kiện rỗng
    /// khi không có sự kiện nào khớp với các tiêu chí tìm kiếm được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetEventsQuery với các tiêu chí tìm kiếm không khớp với bất kỳ sự kiện nào trong DB.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy sự kiện, trả về một danh sách rỗng thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsMatchCriteria()
    {
        // Arrange
        var query = new GetEventsQuery { SearchTerm = "NonExistentEvent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện khớp với tiêu chí tìm kiếm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện vào DB. Tạo một GetEventsQuery với tiêu chí tìm kiếm khớp với các sự kiện đó.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chứa các sự kiện mong đợi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể lọc và trả về
    /// các sự kiện dựa trên các tiêu chí tìm kiếm khác nhau một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEvents_WhenEventsMatchCriteria()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Birthday Party")
            .With(e => e.Location, "New York")
            .With(e => e.Type, EventType.Birth)
            .With(e => e.StartDate, new DateTime(2023, 1, 1))
            .With(e => e.EndDate, new DateTime(2023, 1, 1))
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Wedding Anniversary")
            .With(e => e.Location, "New York")
            .With(e => e.Type, EventType.Marriage)
            .With(e => e.StartDate, new DateTime(2023, 5, 10))
            .With(e => e.EndDate, new DateTime(2023, 5, 10))
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event3 = _fixture.Build<Event>()
            .With(e => e.Name, "Graduation Ceremony")
            .With(e => e.Location, "Los Angeles")
            .With(e => e.Type, EventType.Other)
            .With(e => e.StartDate, new DateTime(2024, 6, 15))
            .With(e => e.EndDate, new DateTime(2024, 6, 15))
            .With(e => e.FamilyId, Guid.NewGuid())
            .Create();

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery
        {
            SearchTerm = "Birthday",
            Location = "New York",
            EventType = EventType.Birth,
            FamilyId = family.Id,
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 1)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be(event1.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả các sự kiện
    /// khi không có tiêu chí tìm kiếm nào được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện vào DB. Tạo một GetEventsQuery rỗng.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chứa tất cả các sự kiện đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể truy xuất
    /// tất cả các sự kiện khi không có bộ lọc nào được áp dụng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoCriteriaSpecified()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Event 1")
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Event 2")
            .With(e => e.FamilyId, family.Id)
            .Create();

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Should().Contain(e => e.Name == event1.Name);
        result.Value!.Should().Contain(e => e.Name == event2.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo loại sự kiện (EventType) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện với các loại khác nhau vào DB. Tạo một GetEventsQuery với một EventType cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện có EventType khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo loại sự kiện hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByEventType()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Birthday Party")
            .With(e => e.Type, EventType.Birth)
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Wedding Anniversary")
            .With(e => e.Type, EventType.Marriage)
            .With(e => e.FamilyId, family.Id)
            .Create();

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery { EventType = EventType.Birth };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be(event1.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo ID gia đình (FamilyId) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện thuộc các gia đình khác nhau vào DB. Tạo một GetEventsQuery với một FamilyId cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện có FamilyId khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo FamilyId hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByFamilyId()
    {
        // Arrange
        var family1 = _fixture.Create<Family>();
        var family2 = _fixture.Create<Family>();
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Family1 Event")
            .With(e => e.FamilyId, family1.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Family2 Event")
            .With(e => e.FamilyId, family2.Id)
            .Create();

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery { FamilyId = family1.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be(event1.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo phạm vi ngày (StartDate và EndDate) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện với các ngày khác nhau vào DB. Tạo một GetEventsQuery với phạm vi ngày cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện nằm trong phạm vi ngày.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo phạm vi ngày hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByDateRange()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Early Event")
            .With(e => e.StartDate, new DateTime(2022, 1, 1))
            .With(e => e.EndDate, new DateTime(2022, 1, 1))
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Middle Event")
            .With(e => e.StartDate, new DateTime(2023, 6, 15))
            .With(e => e.EndDate, new DateTime(2023, 6, 15))
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event3 = _fixture.Build<Event>()
            .With(e => e.Name, "Late Event")
            .With(e => e.StartDate, new DateTime(2024, 12, 31))
            .With(e => e.EndDate, new DateTime(2024, 12, 31))
            .With(e => e.FamilyId, family.Id)
            .Create();

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery
        {
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 12, 31)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be(event2.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo địa điểm (Location) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện với các địa điểm khác nhau vào DB. Tạo một GetEventsQuery với một Location cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện có Location khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo địa điểm hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByLocation()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Event in New York")
            .With(e => e.Location, "New York")
            .With(e => e.FamilyId, family.Id)
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Event in Los Angeles")
            .With(e => e.Location, "Los Angeles")
            .With(e => e.FamilyId, family.Id)
            .Create();

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery { Location = "New York" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be(event1.Name);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo ID thành viên liên quan (RelatedMemberId) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện và thành viên liên quan vào DB. Tạo một GetEventsQuery với một RelatedMemberId cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện có RelatedMemberId khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo RelatedMemberId hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByRelatedMemberId()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1Id = Guid.NewGuid();
        var event1 = new Event { Id = event1Id, FamilyId = familyId, Name = "Event 1", Code = "E001" };
        event1.EventMembers.Add(new EventMember { EventId = event1Id, MemberId = member1.Id });

        var event2Id = Guid.NewGuid();
        var event2 = new Event { Id = event2Id, FamilyId = familyId, Name = "Event 2", Code = "E002" };
        event2.EventMembers.Add(new EventMember { EventId = event2Id, MemberId = member2.Id });

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsQuery { RelatedMemberId = member1.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Id.Should().Be(event1.Id);
    }
}
