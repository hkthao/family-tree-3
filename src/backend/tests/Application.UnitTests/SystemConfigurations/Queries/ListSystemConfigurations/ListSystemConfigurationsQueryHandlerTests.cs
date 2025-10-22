using AutoFixture;
using backend.Application.SystemConfigurations.Queries.ListSystemConfigurations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Queries.ListSystemConfigurations;

public class ListSystemConfigurationsQueryHandlerTests : TestBase
{
    private readonly ListSystemConfigurationsQueryHandler _handler;

    public ListSystemConfigurationsQueryHandlerTests()
    {
        _handler = new ListSystemConfigurationsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithListOfDtos_WhenConfigurationsExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công với danh sách DTO khi các cấu hình tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một danh sách các SystemConfiguration entity vào cơ sở dữ liệu trong bộ nhớ.
        var entities = _fixture.CreateMany<SystemConfiguration>(3).ToList();
        _context.SystemConfigurations.AddRange(entities);
        await _context.SaveChangesAsync();

        // 2. Act: Gọi phương thức Handle.
        var query = new ListSystemConfigurationsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và chứa danh sách DTO chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(entities.Count);
        result.Value!.Select(dto => dto.Key).Should().BeEquivalentTo(entities.Select(e => e.Key));
        // 💡 Giải thích: Handler phải truy xuất tất cả các cấu hình và ánh xạ chúng thành công sang DTO.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenNoConfigurationsExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công với danh sách rỗng khi không có cấu hình nào tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Đảm bảo cơ sở dữ liệu không chứa bất kỳ cấu hình nào.
        // Không thêm bất kỳ cấu hình nào vào _context

        // 2. Act: Gọi phương thức Handle.
        var query = new ListSystemConfigurationsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và chứa danh sách rỗng.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Handler phải trả về một danh sách rỗng khi không có cấu hình nào trong cơ sở dữ liệu.
    }
}
