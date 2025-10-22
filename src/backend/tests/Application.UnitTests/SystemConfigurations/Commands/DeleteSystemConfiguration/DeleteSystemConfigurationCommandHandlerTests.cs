using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.DeleteSystemConfiguration;

public class DeleteSystemConfigurationCommandHandlerTests : TestBase
{
    private readonly DeleteSystemConfigurationCommandHandler _handler;

    public DeleteSystemConfigurationCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new DeleteSystemConfigurationCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteSystemConfiguration_WhenSystemConfigurationExists()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa một SystemConfiguration hiện có.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration vào cơ sở dữ liệu.

        // 2. Act: Gọi phương thức Handle với một DeleteSystemConfigurationCommand có Id của SystemConfiguration đó.

        // 3. Assert: Kiểm tra kết quả trả về là thành công và SystemConfiguration không còn tồn tại trong cơ sở dữ liệu.

        var systemConfiguration = new SystemConfiguration
        {
            Id = Guid.NewGuid(),
            Key = "TestKey",
            Value = "TestValue",
            ValueType = "string",
            Description = "A test system configuration."
        };
        _context.SystemConfigurations.Add(systemConfiguration);
        await _context.SaveChangesAsync();

        var command = new DeleteSystemConfigurationCommand(systemConfiguration.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedConfiguration = await _context.SystemConfigurations.FindAsync(systemConfiguration.Id);
        deletedConfiguration.Should().BeNull();
        // 💡 Giải thích: Handler phải xóa thành công SystemConfiguration khỏi cơ sở dữ liệu khi nó tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSystemConfigurationNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về kết quả thất bại khi SystemConfiguration không tìm thấy.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Tạo một DeleteSystemConfigurationCommand với một Id không tồn tại.

        // 2. Act: Gọi phương thức Handle với lệnh đã tạo.

        // 3. Assert: Kiểm tra kết quả trả về là thất bại và chứa thông báo lỗi phù hợp.

        var nonExistentId = Guid.NewGuid();
        var command = new DeleteSystemConfigurationCommand(nonExistentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"SystemConfiguration with Id {nonExistentId} not found.");
        // 💡 Giải thích: Handler phải trả về lỗi khi cố gắng xóa một SystemConfiguration không tồn tại.
    }
}
