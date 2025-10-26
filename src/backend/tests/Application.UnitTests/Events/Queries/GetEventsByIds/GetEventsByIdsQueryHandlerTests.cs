using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.GetEventsByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandlerTests : TestBase
{
    private readonly GetEventsByIdsQueryHandler _handler;

    public GetEventsByIdsQueryHandlerTests()
    {
        _handler = new GetEventsByIdsQueryHandler(
            _context,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách sự kiện rỗng
    /// khi không có sự kiện nào khớp với các ID được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetEventsByIdsQuery với một danh sách ID không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy sự kiện nào với các ID đã cho, trả về một danh sách rỗng thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsMatchIds()
    {
        // Arrange
        var query = new GetEventsByIdsQuery([Guid.NewGuid(), Guid.NewGuid()]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện khớp với danh sách ID được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số sự kiện vào DB. Tạo một GetEventsByIdsQuery với danh sách ID của các sự kiện đó.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chứa các sự kiện mong đợi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể truy xuất
    /// nhiều sự kiện cụ thể bằng danh sách ID của chúng một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEvents_WhenEventsMatchIds()
    {
        // Arrange
        var event1 = _fixture.Create<Event>();
        var event2 = _fixture.Create<Event>();
        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventsByIdsQuery([event1.Id, event2.Id]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Should().Contain(e => e.Id == event1.Id);
        result.Value!.Should().Contain(e => e.Id == event2.Id);
    }
}
