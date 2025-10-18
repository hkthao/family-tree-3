using backend.Application.Events.Queries.GetEventsByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandlerTests : TestBase
{
    private readonly GetEventsByIdsQueryHandler _handler;

    public GetEventsByIdsQueryHandlerTests()
    {
        _handler = new GetEventsByIdsQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các sự kiện thành công bằng danh sách ID.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách các ID sự kiện hợp lệ,
    /// handler sẽ trả về chính xác các sự kiện tương ứng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEvents_When_IdsExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Thêm một số sự kiện vào cơ sở dữ liệu.
        var event1 = new Event { Id = Guid.NewGuid(), Name = "Sự kiện 1" };
        var event2 = new Event { Id = Guid.NewGuid(), Name = "Sự kiện 2" };
        _context.Events.Add(event1);
        _context.Events.Add(event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với các ID của sự kiện.
        var query = new GetEventsByIdsQuery(new List<Guid> { event1.Id, event2.Id });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về 2 sự kiện.
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(e => e.Id == event1.Id && e.Name == event1.Name);
        result.Value.Should().Contain(e => e.Id == event2.Id && e.Name == event2.Name);
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các sự kiện khi một số ID không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID bao gồm cả ID tồn tại và không tồn tại,
    /// handler sẽ chỉ trả về các sự kiện có ID tồn tại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnOnlyExistingEvents_When_SomeIdsDoNotExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Thêm một sự kiện vào cơ sở dữ liệu.
        var existingEvent = new Event { Id = Guid.NewGuid(), Name = "Sự kiện Tồn Tại" };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với một ID tồn tại và một ID không tồn tại.
        var nonExistentId = Guid.NewGuid();
        var query = new GetEventsByIdsQuery(new List<Guid> { existingEvent.Id, nonExistentId });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và chỉ trả về 1 sự kiện (sự kiện tồn tại).
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var singleEvent = result.Value!.Single();
        singleEvent.Id.Should().Be(existingEvent.Id);
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi không có ID nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID rỗng,
    /// handler sẽ trả về một danh sách sự kiện rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoIdsProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Không cần thêm sự kiện nào.

        // Tạo truy vấn với danh sách ID rỗng.
        var query = new GetEventsByIdsQuery(new List<Guid>());

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về danh sách rỗng.
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
