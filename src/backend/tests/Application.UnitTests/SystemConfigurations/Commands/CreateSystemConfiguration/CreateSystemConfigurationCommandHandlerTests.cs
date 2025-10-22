using AutoFixture.AutoMoq;
using backend.Application.SystemConfigurations.Commands.CreateSystemConfiguration;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.CreateSystemConfiguration;

public class CreateSystemConfigurationCommandHandlerTests : TestBase
{
    private readonly CreateSystemConfigurationCommandHandler _handler;

    public CreateSystemConfigurationCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new CreateSystemConfigurationCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateSystemConfigurationAndReturnId_WhenValidCommand()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo một SystemConfiguration mới và trả về Id của nó khi lệnh hợp lệ.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một CreateSystemConfigurationCommand hợp lệ.

        // 2. Act: Gọi phương thức Handle với lệnh đã tạo.

        // 3. Assert: Kiểm tra kết quả trả về là thành công, chứa một Guid hợp lệ, và SystemConfiguration đã được thêm vào cơ sở dữ liệu.

        var command = new CreateSystemConfigurationCommand
        {
            Key = "TestKey",
            Value = "TestValue",
            ValueType = "string",
            Description = "A test system configuration."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdConfiguration = await _context.SystemConfigurations.FindAsync(result.Value);
        createdConfiguration.Should().NotBeNull();
        createdConfiguration!.Key.Should().Be(command.Key);
        createdConfiguration.Value.Should().Be(command.Value);
        createdConfiguration.ValueType.Should().Be(command.ValueType);
        createdConfiguration.Description.Should().Be(command.Description);
        // 💡 Giải thích: Handler phải tạo thành công một SystemConfiguration mới với các thuộc tính được cung cấp và trả về Id của nó.
    }

    [Fact]
    public async Task Handle_ShouldCreateSystemConfigurationWithEmptyKey_WhenEmptyKeyProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo một SystemConfiguration ngay cả khi Key trống.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một CreateSystemConfigurationCommand với Key trống.

        // 2. Act: Gọi phương thức Handle với lệnh đã tạo.

        // 3. Assert: Kiểm tra kết quả trả về là thành công, chứa một Guid hợp lệ, và SystemConfiguration đã được thêm vào cơ sở dữ liệu với Key trống.

        var command = new CreateSystemConfigurationCommand
        {
            Key = string.Empty,
            Value = "AnotherTestValue",
            ValueType = "string",
            Description = "A system configuration with an empty key."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdConfiguration = await _context.SystemConfigurations.FindAsync(result.Value);
        createdConfiguration.Should().NotBeNull();
        createdConfiguration!.Key.Should().Be(command.Key);
        createdConfiguration.Value.Should().Be(command.Value);
        createdConfiguration.ValueType.Should().Be(command.ValueType);
        createdConfiguration.Description.Should().Be(command.Description);
        // 💡 Giải thích: Handler hiện tại cho phép tạo SystemConfiguration với Key trống. Test này xác minh hành vi đó.
    }
}
