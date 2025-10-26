using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.SearchEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Queries.SearchEvents;

public class SearchEventsQueryHandlerTests : TestBase
{
    private readonly SearchEventsQueryHandler _handler;

    public SearchEventsQueryHandlerTests()
    {
        _handler = new SearchEventsQueryHandler(
            _context,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách phân trang rỗng
    /// khi không có sự kiện nào khớp với các tiêu chí tìm kiếm được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một SearchEventsQuery với các tiêu chí tìm kiếm không khớp với bất kỳ sự kiện nào trong DB.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách phân trang là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy sự kiện, trả về một danh sách rỗng thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoEventsMatchCriteria()
    {
        // Arrange
        var query = new SearchEventsQuery { SearchQuery = "NonExistentEvent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value!.TotalItems.Should().Be(0);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện được phân trang khớp với tiêu chí tìm kiếm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện vào DB. Tạo một SearchEventsQuery với tiêu chí tìm kiếm khớp với các sự kiện đó.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách phân trang chứa các sự kiện mong đợi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể lọc và trả về
    /// các sự kiện dựa trên các tiêu chí tìm kiếm và phân trang một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnPaginatedEvents_WhenEventsMatchCriteria()
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.Name, "Birthday Party")
            .With(e => e.Location, "New York")
            .With(e => e.Type, EventType.Birth)
            .With(e => e.FamilyId, family.Id)
            .With(e => e.StartDate, new DateTime(2023, 1, 1))
            .With(e => e.EndDate, new DateTime(2023, 1, 1))
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.Name, "Wedding Anniversary")
            .With(e => e.Location, "New York")
            .With(e => e.Type, EventType.Marriage)
            .With(e => e.FamilyId, family.Id)
            .With(e => e.StartDate, new DateTime(2023, 5, 10))
            .Create();
        var event3 = _fixture.Build<Event>()
            .With(e => e.Name, "Graduation Ceremony")
            .With(e => e.Location, "Los Angeles")
            .With(e => e.Type, EventType.Other)
            .With(e => e.FamilyId, Guid.NewGuid())
            .With(e => e.StartDate, new DateTime(2024, 6, 15))
            .Create();

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery
        {
            SearchQuery = "Birthday",
            Type = EventType.Birth.ToString(),
            FamilyId = family.Id,
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 1),
            Page = 1,
            ItemsPerPage = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.First().Name.Should().Be(event1.Name);
        result.Value!.TotalItems.Should().Be(1);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả các sự kiện được phân trang
    /// khi không có tiêu chí tìm kiếm nào được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện vào DB. Tạo một SearchEventsQuery rỗng với thông tin phân trang.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách phân trang chứa tất cả các sự kiện đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể truy xuất
    /// tất cả các sự kiện khi không có bộ lọc nào được áp dụng và phân trang hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllEventsPaginated_WhenNoCriteriaSpecified()
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
        var event3 = _fixture.Build<Event>()
            .With(e => e.Name, "Event 3")
            .With(e => e.FamilyId, family.Id)
            .Create();

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery
        {
            Page = 1,
            ItemsPerPage = 2
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.TotalItems.Should().Be(3);
        result.Value!.TotalPages.Should().Be(2);
    }
}
