using backend.Application.Events.Queries.SearchEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.SearchEvents;

public class SearchEventsQueryHandlerTests : TestBase
{
    private readonly SearchEventsQueryHandler _handler;

    public SearchEventsQueryHandlerTests()
    {
        _handler = new SearchEventsQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// Kiểm tra tìm kiếm sự kiện theo truy vấn tìm kiếm (tên hoặc mô tả).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các sự kiện dựa trên một chuỗi tìm kiếm
    /// khớp với tên hoặc mô tả của sự kiện.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEvents_When_SearchingByQuery()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện A", Description = "Mô tả A", FamilyId = familyId, Code = "EVT001" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện B", Description = "Mô tả B", FamilyId = familyId, Code = "EVT002" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện C", Description = "Mô tả C", FamilyId = familyId, Code = "EVT003" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { SearchQuery = "Sự kiện A", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Sự kiện A");
    }

    /// <summary>
    /// Kiểm tra tìm kiếm sự kiện theo FamilyId.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các sự kiện dựa trên ID của gia đình.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEvents_When_SearchingByFamilyId()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Gia đình 1", FamilyId = familyId1, Code = "EVT001" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Gia đình 2", FamilyId = familyId2, Code = "EVT002" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { FamilyId = familyId1, Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Sự kiện Gia đình 1");
    }

    /// <summary>
    /// Kiểm tra tìm kiếm sự kiện theo phạm vi ngày.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các sự kiện dựa trên ngày bắt đầu và ngày kết thúc.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEvents_When_SearchingByDateRange()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Hôm Qua", StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(-1), FamilyId = familyId, Code = "EVT001" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Hôm Nay", StartDate = DateTime.Now, EndDate = DateTime.Now, FamilyId = familyId, Code = "EVT002" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Ngày Mai", StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now.AddDays(1), FamilyId = familyId, Code = "EVT003" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { StartDate = DateTime.Now.AddHours(-1), EndDate = DateTime.Now.AddHours(1), Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Sự kiện Hôm Nay");
    }

    /// <summary>
    /// Kiểm tra tìm kiếm sự kiện với nhiều tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể kết hợp nhiều tiêu chí tìm kiếm (ví dụ: truy vấn và FamilyId)
    /// để lọc các sự kiện.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEvents_When_SearchingByMultipleCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện A Gia đình 1", FamilyId = familyId1, Code = "EVT001" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện B Gia đình 1", FamilyId = familyId1, Code = "EVT002" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện A Gia đình 2", FamilyId = familyId2, Code = "EVT003" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { SearchQuery = "Sự kiện A", FamilyId = familyId1, Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Sự kiện A Gia đình 1");
    }

    /// <summary>
    /// Kiểm tra trả về danh sách rỗng khi không có sự kiện nào khớp với tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi không có sự kiện nào trong cơ sở dữ liệu khớp với các tiêu chí tìm kiếm,
    /// handler sẽ trả về một danh sách sự kiện rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoEventsMatchCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện Duy Nhất", FamilyId = familyId, Code = "EVT001" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { SearchQuery = "Sự kiện Không Tồn Tại", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trả về tất cả các sự kiện khi không có tiêu chí tìm kiếm nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một truy vấn tìm kiếm rỗng được gửi đi,
    /// handler sẽ trả về tất cả các sự kiện có trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnAllEvents_When_NoSearchCriteriaProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện 1", FamilyId = familyId, Code = "EVT001" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện 2", FamilyId = familyId, Code = "EVT002" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchEventsQuery { Page = 1, ItemsPerPage = 10 }; // Không có tiêu chí tìm kiếm

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }
}
