using AutoFixture.AutoMoq;
using backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.UpdateSystemConfiguration;

public class UpdateSystemConfigurationCommandHandlerTests : TestBase
{
    private readonly UpdateSystemConfigurationCommandHandler _handler;

    public UpdateSystemConfigurationCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new UpdateSystemConfigurationCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSystemConfiguration_WhenSystemConfigurationExists()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cập nhật một SystemConfiguration hiện có.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration vào cơ sở dữ liệu.

        // 2. Act: Tạo một UpdateSystemConfigurationCommand với các giá trị mới và Id của SystemConfiguration đó, sau đó gọi phương thức Handle.

        // 3. Assert: Kiểm tra kết quả trả về là thành công và SystemConfiguration trong cơ sở dữ liệu đã được cập nhật với các giá trị mới.

        var systemConfiguration = new SystemConfiguration
        {
            Id = Guid.NewGuid(),
            Key = "OriginalKey",
            Value = "OriginalValue",
            ValueType = "string",
            Description = "Original description."
        };
        _context.SystemConfigurations.Add(systemConfiguration);
        await _context.SaveChangesAsync();

        var updatedKey = "UpdatedKey";
        var updatedValue = "UpdatedValue";
        var updatedValueType = "integer";
        var updatedDescription = "Updated description.";

        var command = new UpdateSystemConfigurationCommand
        {
            Id = systemConfiguration.Id,
            Key = updatedKey,
            Value = updatedValue,
            ValueType = updatedValueType,
            Description = updatedDescription
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedConfiguration = await _context.SystemConfigurations.FindAsync(systemConfiguration.Id);
        updatedConfiguration.Should().NotBeNull();
        updatedConfiguration!.Key.Should().Be(updatedKey);
        updatedConfiguration.Value.Should().Be(updatedValue);
        updatedConfiguration.ValueType.Should().Be(updatedValueType);
        updatedConfiguration.Description.Should().Be(updatedDescription);
        // 💡 Giải thích: Handler phải cập nhật thành công SystemConfiguration với các giá trị mới khi nó tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSystemConfigurationNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về kết quả thất bại khi SystemConfiguration không tìm thấy.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một UpdateSystemConfigurationCommand với một Id không tồn tại.

        // 2. Act: Gọi phương thức Handle với lệnh đã tạo.

        // 3. Assert: Kiểm tra kết quả trả về là thất bại và chứa thông báo lỗi phù hợp.

        var nonExistentId = Guid.NewGuid();
        var command = new UpdateSystemConfigurationCommand
        {
            Id = nonExistentId,
            Key = "AnyKey",
            Value = "AnyValue",
            ValueType = "string",
            Description = "Any description."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"SystemConfiguration with Id {nonExistentId} not found.");
        // 💡 Giải thích: Handler phải trả về lỗi khi cố gắng cập nhật một SystemConfiguration không tồn tại.
    }
}
