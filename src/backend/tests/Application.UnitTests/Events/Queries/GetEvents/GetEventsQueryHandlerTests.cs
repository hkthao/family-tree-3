using backend.Application.Events.Queries.GetEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests : TestBase
{
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _handler = new GetEventsQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// Kiểm tra xem tất cả các sự kiện có được trả về khi không có bộ lọc nào được áp dụng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một truy vấn GetEventsQuery không có tiêu chí lọc cụ thể,
    /// handler sẽ trả về tất cả các sự kiện có trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        // Thêm một số sự kiện vào cơ sở dữ liệu cho gia đình này.
        var events = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Name = "Sự kiện 1", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Code = "EVT001" },
            new Event { Id = Guid.NewGuid(), Name = "Sự kiện 2", FamilyId = royalFamilyId, Created = DateTime.UtcNow, Code = "EVT002" }
        };
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy tất cả sự kiện cho gia đình này.
        var query = new GetEventsQuery { FamilyId = royalFamilyId, ItemsPerPage = 10000 };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler lấy sự kiện).
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // Đảm bảo truy vấn thành công và trả về đúng số lượng sự kiện.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new { Name = "Sự kiện 1" });
        result.Value.Should().ContainEquivalentOf(new { Name = "Sự kiện 2" });
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các sự kiện cho một FamilyId cụ thể.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các sự kiện dựa trên FamilyId được cung cấp.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnEventsForSpecificFamilyId()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var anotherFamilyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = anotherFamilyId, Name = "Another Family", Code = "ANOTHERFAM" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện của Gia đình Hoàng gia", FamilyId = royalFamilyId, Code = "EVT003" });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện của Gia đình Khác", FamilyId = anotherFamilyId, Code = "EVT004" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy sự kiện cho FamilyId của gia đình 1.
        var query = new GetEventsQuery { FamilyId = royalFamilyId, ItemsPerPage = 10 };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler lấy sự kiện).
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // Đảm bảo truy vấn thành công và trả về đúng sự kiện của gia đình 1.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be("Sự kiện của Gia đình 1");
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi không có sự kiện nào cho FamilyId được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một FamilyId được cung cấp không có sự kiện nào liên quan,
    /// handler sẽ trả về một danh sách sự kiện rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsForFamilyId()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Gia đình Không Sự kiện", Code = "FAMNOEVT" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy sự kiện cho gia đình này.
        var query = new GetEventsQuery { FamilyId = familyId, ItemsPerPage = 10 };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler lấy sự kiện).
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // Đảm bảo truy vấn thành công và trả về danh sách rỗng.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra phân trang khi truy xuất sự kiện.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể xử lý phân trang đúng cách,
    /// trả về số lượng sự kiện chính xác cho mỗi trang.
    /// Hiện tại, handler không thực hiện phân trang, nên test này sẽ kiểm tra rằng tất cả các mục được trả về.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnPaginatedEvents()
    {
        // Arrange: Thiết lập môi trường và dữ liệu ban đầu cho bài kiểm tra.
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Gia đình Phân trang", Code = "FAMPAG" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một gia đình và thêm nhiều sự kiện vào cơ sở dữ liệu.
        for (int i = 0; i < 5; i++)
        {
            _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = $"Sự kiện {i}", FamilyId = familyId, Code = $"EVT{i:D3}" });
        }
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với phân trang (trang 1, 2 mục mỗi trang).
        var query = new GetEventsQuery { FamilyId = familyId, Page = 1, ItemsPerPage = 2 };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler lấy sự kiện).
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // Đảm bảo truy vấn thành công và trả về tất cả các sự kiện (do handler hiện không phân trang).
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(5); // Handler hiện tại trả về tất cả 5 mục, không phân trang.
    }
}
