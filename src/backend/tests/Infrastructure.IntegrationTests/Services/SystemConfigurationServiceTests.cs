using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace backend.Infrastructure.IntegrationTests.Services;

[Collection(nameof(IntegrationTestCollection))]
public class SystemConfigurationServiceTests : IntegrationTestBase
{
    private readonly SystemConfigurationService _service;
    private readonly Mock<ILogger<SystemConfigurationService>> _mockLogger;

    public SystemConfigurationServiceTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _mockLogger = new Mock<ILogger<SystemConfigurationService>>();
        _service = new SystemConfigurationService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnConfiguration_WhenExists()
    {
        // 🎯 Mục tiêu: Xác minh GetConfigurationAsync trả về cấu hình khi nó tồn tại trong DB.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration vào DB.
        var key = "TestKey1";
        var expectedConfig = new SystemConfiguration
        {
            Key = key,
            Value = "TestValue1",
            ValueType = "string",
            Description = "Description 1",
            Created = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        _dbContext.SystemConfigurations.Add(expectedConfig);
        await _dbContext.SaveChangesAsync();

        // 2. Act: Gọi GetConfigurationAsync với khóa của cấu hình đó.
        var result = await _service.GetConfigurationAsync(key);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và chứa cấu hình chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Key.Should().Be(key);
        result.Value.Value.Should().Be("TestValue1");
        // 💡 Giải thích: Dịch vụ phải truy xuất thành công cấu hình đã lưu từ cơ sở dữ liệu.
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnFailure_WhenNotExists()
    {
        // 🎯 Mục tiêu: Xác minh GetConfigurationAsync trả về thất bại khi cấu hình không tồn tại trong DB.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Đảm bảo DB không chứa cấu hình với khóa được yêu cầu.
        var key = "NonExistentKey";

        // 2. Act: Gọi GetConfigurationAsync với khóa không tồn tại.
        var result = await _service.GetConfigurationAsync(key);

        // 3. Assert: Kiểm tra rằng Result trả về là thất bại và chứa thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Configuration with key '{key}' not found.");
        // 💡 Giải thích: Dịch vụ phải báo cáo lỗi khi không tìm thấy cấu hình.
    }

    [Fact]
    public async Task GetAllConfigurationsAsync_ShouldReturnAllConfigurations()
    {
        // 🎯 Mục tiêu: Xác minh GetAllConfigurationsAsync trả về tất cả các cấu hình trong DB.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một vài SystemConfiguration vào DB.
        var config1 = new SystemConfiguration { Key = "Key1", Value = "Value1", ValueType = "string", Created = DateTime.UtcNow, CreatedBy = "TestSystem" };
        var config2 = new SystemConfiguration { Key = "Key2", Value = "Value2", ValueType = "string", Created = DateTime.UtcNow, CreatedBy = "TestSystem" };
        _dbContext.SystemConfigurations.AddRange(config1, config2);
        await _dbContext.SaveChangesAsync();

        // 2. Act: Gọi GetAllConfigurationsAsync.
        var result = await _service.GetAllConfigurationsAsync();

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và chứa tất cả các cấu hình đã thêm.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Key == "Key1");
        result.Value.Should().Contain(c => c.Key == "Key2");
        // 💡 Giải thích: Dịch vụ phải truy xuất tất cả các cấu hình đã lưu từ cơ sở dữ liệu.
    }

    [Fact]
    public async Task SetConfigurationAsync_ShouldAddConfiguration_WhenNotExists()
    {
        // 🎯 Mục tiêu: Xác minh SetConfigurationAsync thêm cấu hình mới khi nó chưa tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Chuẩn bị dữ liệu cấu hình mới.
        var key = "NewKey";
        var value = "NewValue";
        var valueType = "string";
        var description = "New Description";

        // 2. Act: Gọi SetConfigurationAsync với khóa mới.
        var result = await _service.SetConfigurationAsync(key, value, valueType, description);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và cấu hình mới đã được thêm vào DB.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var addedConfig = await _dbContext.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        addedConfig.Should().NotBeNull();
        addedConfig!.Key.Should().Be(key);
        addedConfig.Value.Should().Be(value);
        addedConfig.ValueType.Should().Be(valueType);
        addedConfig.Description.Should().Be(description);
        // 💡 Giải thích: Dịch vụ phải thêm một cấu hình mới vào cơ sở dữ liệu khi nó không tồn tại.
    }

    [Fact]
    public async Task SetConfigurationAsync_ShouldUpdateConfiguration_WhenExists()
    {
        // 🎯 Mục tiêu: Xác minh SetConfigurationAsync cập nhật cấu hình hiện có.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration vào DB.
        var key = "UpdateKey";
        var originalValue = "OriginalValue";
        var updatedValue = "UpdatedValue";
        var configToUpdate = new SystemConfiguration
        {
            Key = key,
            Value = originalValue,
            ValueType = "string",
            Description = "Original Description",
            Created = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        _dbContext.SystemConfigurations.Add(configToUpdate);
        await _dbContext.SaveChangesAsync();

        // 2. Act: Gọi SetConfigurationAsync với khóa của cấu hình đó và giá trị mới.
        var result = await _service.SetConfigurationAsync(key, updatedValue, "string", "Updated Description");

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và cấu hình trong DB đã được cập nhật.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedConfig = await _dbContext.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        updatedConfig.Should().NotBeNull();
        updatedConfig!.Value.Should().Be(updatedValue);
        updatedConfig.Description.Should().Be("Updated Description");
        // 💡 Giải thích: Dịch vụ phải cập nhật giá trị của cấu hình hiện có trong cơ sở dữ liệu.
    }

    [Fact]
    public async Task DeleteConfigurationAsync_ShouldDeleteConfiguration_WhenExists()
    {
        // 🎯 Mục tiêu: Xác minh DeleteConfigurationAsync xóa cấu hình khi nó tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thêm một SystemConfiguration vào DB.
        var key = "DeleteKey";
        var configToDelete = new SystemConfiguration
        {
            Key = key,
            Value = "ValueToDelete",
            ValueType = "string",
            Created = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        _dbContext.SystemConfigurations.Add(configToDelete);
        await _dbContext.SaveChangesAsync();

        // 2. Act: Gọi DeleteConfigurationAsync với khóa của cấu hình đó.
        var result = await _service.DeleteConfigurationAsync(key);

        // 3. Assert: Kiểm tra rằng Result trả về là thành công và cấu hình đã bị xóa khỏi DB.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedConfig = await _dbContext.SystemConfigurations.FirstOrDefaultAsync(sc => sc.Key == key);
        deletedConfig.Should().BeNull();
        // 💡 Giải thích: Dịch vụ phải xóa cấu hình khỏi cơ sở dữ liệu khi nó tồn tại.
    }

    [Fact]
    public async Task DeleteConfigurationAsync_ShouldReturnFailure_WhenNotExists()
    {
        // 🎯 Mục tiêu: Xác minh DeleteConfigurationAsync trả về thất bại khi cấu hình không tồn tại.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Đảm bảo DB không chứa cấu hình với khóa được yêu cầu.
        var key = "NonExistentDeleteKey";

        // 2. Act: Gọi DeleteConfigurationAsync với khóa không tồn tại.
        var result = await _service.DeleteConfigurationAsync(key);

        // 3. Assert: Kiểm tra rằng Result trả về là thất bại và chứa thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Configuration with key '{key}' not found.");
        // 💡 Giải thích: Dịch vụ phải báo cáo lỗi khi cố gắng xóa một cấu hình không tồn tại.
    }
}
