using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventById;

public class GetEventByIdQueryHandlerTests : TestBase
{
    private readonly GetEventByIdQueryHandler _handler;

    public GetEventByIdQueryHandlerTests()
    {
        _handler = new GetEventByIdQueryHandler(
            _context,
            _mapper
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy sự kiện với ID được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetEventByIdQuery với một ID không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không tìm thấy sự kiện, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var query = new GetEventByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.EventNotFound, query.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về sự kiện thành công
    /// khi tìm thấy sự kiện với ID được cung cấp.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một sự kiện và thêm vào DB. Tạo một GetEventByIdQuery với ID của sự kiện đó.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và sự kiện trả về khớp với sự kiện đã tạo.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể truy xuất
    /// một sự kiện cụ thể bằng ID của nó một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventSuccessfully_WhenEventFound()
    {
        // Arrange
        var eventEntity = _fixture.Create<Event>();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetEventByIdQuery(eventEntity.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(eventEntity.Id);
        result.Value.Name.Should().Be(eventEntity.Name);
    }
}
