using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.SystemConfigurations.Commands.InitializeSystemConfigurations;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.SystemConfigurations.Commands.InitializeSystemConfigurations;

public class InitializeSystemConfigurationsCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<InitializeSystemConfigurationsCommandHandler>> _mockLogger;
    private readonly Mock<ISystemConfigurationService> _mockSystemConfigurationService;
    private readonly IConfiguration _configuration;
    private readonly InitializeSystemConfigurationsCommandHandler _handler;

    public InitializeSystemConfigurationsCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _mockLogger = new Mock<ILogger<InitializeSystemConfigurationsCommandHandler>>();
        _mockSystemConfigurationService = new Mock<ISystemConfigurationService>();

        // Use ConfigurationBuilder for IConfiguration
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection();
        _configuration = builder.Build();

        _handler = new InitializeSystemConfigurationsCommandHandler(
            _mockLogger.Object,
            _mockSystemConfigurationService.Object,
            _configuration);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenConfigurationsAlreadyExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công và bỏ qua khởi tạo khi các cấu hình đã tồn tại trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockSystemConfigurationService để trả về các cấu hình hiện có.

        // 2. Act: Gọi phương thức Handle.

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và SetConfigurationsAsync không được gọi.

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var handler = new InitializeSystemConfigurationsCommandHandler(
            _mockLogger.Object,
            _mockSystemConfigurationService.Object,
            configuration);

        _mockSystemConfigurationService.Setup(s => s.GetAllConfigurationsAsync())
            .ReturnsAsync(Result<List<SystemConfiguration>>.Success(new List<SystemConfiguration> { new SystemConfiguration() }));

        var result = await handler.Handle(new InitializeSystemConfigurationsCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockSystemConfigurationService.Verify(s => s.SetConfigurationsAsync(It.IsAny<Dictionary<string, (string value, string valueType, string description)>>()), Times.Never);
        // 💡 Giải thích: Nếu các cấu hình đã tồn tại, handler phải trả về thành công mà không cố gắng khởi tạo lại.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNoConfigurationsInIConfiguration()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành công và không gọi SetConfigurationsAsync khi không có cấu hình nào trong IConfiguration.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockSystemConfigurationService để trả về không có cấu hình nào hiện có.
        //             Thiết lập _mockConfiguration để trả về một cấu hình trống.

        // 2. Act: Gọi phương thức Handle.

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và SetConfigurationsAsync không được gọi.

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var handler = new InitializeSystemConfigurationsCommandHandler(
            _mockLogger.Object,
            _mockSystemConfigurationService.Object,
            configuration);

        _mockSystemConfigurationService.Setup(s => s.GetAllConfigurationsAsync())
            .ReturnsAsync(Result<List<SystemConfiguration>>.Success(new List<SystemConfiguration>()));

        var result = await handler.Handle(new InitializeSystemConfigurationsCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockSystemConfigurationService.Verify(s => s.SetConfigurationsAsync(It.IsAny<Dictionary<string, (string value, string valueType, string description)>>()), Times.Never);
        // 💡 Giải thích: Nếu không có cấu hình nào trong IConfiguration, handler phải trả về thành công mà không cố gắng lưu bất cứ điều gì.
    }

    [Fact]
    public async Task Handle_ShouldInitializeConfigurationsSuccessfully_WhenConfigurationsDoNotExist()
    {
        // 🎯 Mục tiêu của test: Xác minh handler khởi tạo các cấu hình thành công khi chúng chưa tồn tại trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockSystemConfigurationService để trả về không có cấu hình nào hiện có.
        //             Thiết lập _mockConfiguration để trả về một số cấu hình.
        //             Thiết lập _mockSystemConfigurationService để trả về thành công khi SetConfigurationsAsync được gọi.

        // 2. Act: Gọi phương thức Handle.

        // 3. Assert: Kiểm tra rằng kết quả trả về là thành công và SetConfigurationsAsync được gọi với các cấu hình chính xác.

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Config1", "Value1"},
                {"Config2", "123"},
                {"Config3", "true"}
            })
            .Build();
        var handler = new InitializeSystemConfigurationsCommandHandler(
            _mockLogger.Object,
            _mockSystemConfigurationService.Object,
            configuration);

        _mockSystemConfigurationService.Setup(s => s.GetAllConfigurationsAsync())
            .ReturnsAsync(Result<List<SystemConfiguration>>.Success(new List<SystemConfiguration>()));

        _mockSystemConfigurationService.Setup(s => s.SetConfigurationsAsync(It.IsAny<Dictionary<string, (string value, string valueType, string description)>>()))
            .ReturnsAsync(Result.Success());

        var result = await handler.Handle(new InitializeSystemConfigurationsCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockSystemConfigurationService.Verify(s => s.SetConfigurationsAsync(It.Is<Dictionary<string, (string value, string valueType, string description)>>(dict =>
            dict.Count == 3 &&
            dict["Config1"].value == "Value1" && dict["Config1"].valueType == "string" &&
            dict["Config2"].value == "123" && dict["Config2"].valueType == "int" &&
            dict["Config3"].value == "true" && dict["Config3"].valueType == "bool"
        )), Times.Once);
        // 💡 Giải thích: Handler phải đọc các cấu hình từ IConfiguration, xác định loại của chúng và lưu chúng vào cơ sở dữ liệu.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSetConfigurationsAsyncFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thất bại khi SetConfigurationsAsync không thành công.

        // ⚙️ Các bước (Arrange, Act, Assert):

        // 1. Arrange: Thiết lập _mockSystemConfigurationService để trả về không có cấu hình nào hiện có.
        //             Thiết lập _mockConfiguration để trả về một số cấu hình.
        //             Thiết lập _mockSystemConfigurationService để trả về thất bại khi SetConfigurationsAsync được gọi.

        // 2. Act: Gọi phương thức Handle.

        // 3. Assert: Kiểm tra rằng kết quả trả về là thất bại và chứa thông báo lỗi phù hợp.

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Config1", "Value1"}
            })
            .Build();
        var handler = new InitializeSystemConfigurationsCommandHandler(
            _mockLogger.Object,
            _mockSystemConfigurationService.Object,
            configuration);

        _mockSystemConfigurationService.Setup(s => s.GetAllConfigurationsAsync())
            .ReturnsAsync(Result<List<SystemConfiguration>>.Success(new List<SystemConfiguration>()));

        var errorMessage = "Failed to save configurations.";
        _mockSystemConfigurationService.Setup(s => s.SetConfigurationsAsync(It.IsAny<Dictionary<string, (string value, string valueType, string description)>>()))
            .ReturnsAsync(Result.Failure(errorMessage));

        var result = await handler.Handle(new InitializeSystemConfigurationsCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(errorMessage);
        // 💡 Giải thích: Nếu việc lưu cấu hình thất bại, handler phải trả về lỗi.
    }
}