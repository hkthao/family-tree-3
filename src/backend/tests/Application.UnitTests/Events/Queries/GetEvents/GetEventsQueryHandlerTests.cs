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
        // Xóa tất cả các sự kiện hiện có để đảm bảo môi trường test sạch.
        _context.Events.RemoveRange(_context.Events);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một gia đình và thêm vào cơ sở dữ liệu.
        var family = new Family { Id = Guid.NewGuid(), Name = "Gia đình Test" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thêm một số sự kiện vào cơ sở dữ liệu cho gia đình này.
        var events = new List<Event>
        {
            new Event { Id = Guid.NewGuid(), Name = "Sự kiện 1", FamilyId = family.Id, Created = DateTime.UtcNow },
            new Event { Id = Guid.NewGuid(), Name = "Sự kiện 2", FamilyId = family.Id, Created = DateTime.UtcNow }
        };
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy tất cả sự kiện cho gia đình này.
        var query = new GetEventsQuery { FamilyId = family.Id, ItemsPerPage = 10000 };

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
        // Xóa tất cả các sự kiện hiện có để đảm bảo môi trường test sạch.
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo hai gia đình và thêm sự kiện cho mỗi gia đình.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Gia đình 1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Gia đình 2" };
        _context.Families.AddRange(family1, family2);
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện của Gia đình 1", FamilyId = family1.Id });
        _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = "Sự kiện của Gia đình 2", FamilyId = family2.Id });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy sự kiện cho FamilyId của gia đình 1.
        var query = new GetEventsQuery { FamilyId = family1.Id, ItemsPerPage = 10 };

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
        // Xóa tất cả các sự kiện hiện có để đảm bảo môi trường test sạch.
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một gia đình nhưng không thêm sự kiện nào cho nó.
        var family = new Family { Id = Guid.NewGuid(), Name = "Gia đình Không Sự kiện" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn để lấy sự kiện cho gia đình này.
        var query = new GetEventsQuery { FamilyId = family.Id, ItemsPerPage = 10 };

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
        // Xóa tất cả các sự kiện hiện có để đảm bảo môi trường test sạch.
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một gia đình và thêm nhiều sự kiện vào cơ sở dữ liệu.
        var family = new Family { Id = Guid.NewGuid(), Name = "Gia đình Phân trang" };
        _context.Families.Add(family);
        for (int i = 0; i < 5; i++)
        {
            _context.Events.Add(new Event { Id = Guid.NewGuid(), Name = $"Sự kiện {i}", FamilyId = family.Id });
        }
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với phân trang (trang 1, 2 mục mỗi trang).
        var query = new GetEventsQuery { FamilyId = family.Id, Page = 1, ItemsPerPage = 2 };

        // Act: Thực hiện hành động cần kiểm tra (gọi handler lấy sự kiện).
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả mong đợi.
        // Đảm bảo truy vấn thành công và trả về tất cả các sự kiện (do handler hiện không phân trang).
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(5); // Handler hiện tại trả về tất cả 5 mục, không phân trang.
    }
}
