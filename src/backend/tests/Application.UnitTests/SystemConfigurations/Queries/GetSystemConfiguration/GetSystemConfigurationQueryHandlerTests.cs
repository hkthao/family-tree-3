using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using backend.Application.Common.Models;
using backend.Application.SystemConfigurations.Queries;
using backend.Application.SystemConfigurations.Queries.GetSystemConfiguration;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Queries.GetSystemConfiguration;

public class GetSystemConfigurationQueryHandlerTests : TestBase
{
    private readonly GetSystemConfigurationQueryHandler _handler;

    public GetSystemConfigurationQueryHandlerTests()
    {
        _handler = new GetSystemConfigurationQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithDto_WhenConfigurationExists()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công với DTO khi cấu hình tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration entity vào cơ sở dữ liệu trong bộ nhớ.
        var key = "TestKey";
        var entity = _fixture.Build<SystemConfiguration>()
            .With(sc => sc.Key, key)
            .Create();
        _context.SystemConfigurations.Add(entity);
        await _context.SaveChangesAsync();

        // 2. Act: Gọi phương thức Handle.
        var query = new GetSystemConfigurationQuery(key);
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và chứa DTO chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Key.Should().Be(key);
        result.Value.Id.Should().Be(entity.Id);
        result.Value.Value.Should().Be(entity.Value);
        result.Value.Description.Should().Be(entity.Description);
        result.Value.ValueType.Should().Be(entity.ValueType);
        // 💡 Giải thích: Handler phải tìm thấy cấu hình và ánh xạ nó thành công sang DTO.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenConfigurationDoesNotExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại khi cấu hình không tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Đảm bảo cơ sở dữ liệu không chứa cấu hình với khóa được yêu cầu.
        var key = "NonExistentKey";
        // Không thêm bất kỳ cấu hình nào vào _context

        // 2. Act: Gọi phương thức Handle.
        var query = new GetSystemConfigurationQuery(key);
        var result = await _handler.Handle(query, CancellationToken.None);

        // 3. Assert: Kiểm tra rằng kết quả trả về là thất bại và chứa thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"SystemConfiguration with Key {key} not found.");
        // 💡 Giải thích: Handler phải báo cáo lỗi khi không tìm thấy cấu hình.
    }
}
