using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.NaturalLanguageInput.Commands.GenerateData;
using backend.Domain.Enums;
using System.Text.Json;

namespace backend.Application.UnitTests.NaturalLanguageInput.Commands.GenerateData;

public class GenerateEventDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly GenerateEventDataCommandHandler _handler;

    public GenerateEventDataCommandHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockChatProvider = new Mock<IChatProvider>();
        _fixture.Customize(new AutoMoqCustomization());

        _mockChatProviderFactory.Setup(f => f.GetProvider(ChatAIProvider.Local))
            .Returns(_mockChatProvider.Object);

        _handler = new GenerateEventDataCommandHandler(
            _mockChatProviderFactory.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEventDataSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về dữ liệu sự kiện thành công khi AI cung cấp JSON hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ chứa danh sách EventDto.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa danh sách EventDto mong đợi.
        var prompt = "Lên lịch một buổi họp mặt gia đình vào ngày 2025-01-01 tại nhà.";
        var expectedEvents = new List<EventDto>
        {
            new() { Name = "Buổi họp mặt gia đình", StartDate = new DateTime(2025, 1, 1), Location = "Nhà", Type = EventType.Other }
        };
        var aiResponseJson = JsonSerializer.Serialize(new { events = expectedEvents }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(aiResponseJson);

        var command = new GenerateEventDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be("Buổi họp mặt gia đình");
        // 💡 Giải thích: Handler phải phân tích cú pháp phản hồi JSON hợp lệ từ AI và trả về danh sách EventDto.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi phản hồi từ AI là rỗng hoặc chỉ chứa khoảng trắng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về một chuỗi rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var prompt = "Generate some event data.";
        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(string.Empty);

        var command = new GenerateEventDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI did not return a response.");
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không trả về bất kỳ dữ liệu nào.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseIsInvalidJson()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi phản hồi từ AI là JSON không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var prompt = "Generate some event data.";
        var invalidJson = "{ \"events\": [ { \"title\": \"Invalid JSON\", \"date\": \"2025-01-01\" } "; // Malformed JSON

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(invalidJson);

        var command = new GenerateEventDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid JSON");
        // 💡 Giải thích: Handler phải bắt lỗi JsonException khi phản hồi không phải là JSON hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseContainsEmptyEventsArray()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi phản hồi từ AI chứa một mảng 'events' rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ nhưng với mảng 'events' rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var prompt = "Generate some event data.";
        var emptyEventsJson = "{ \"events\": [] }";

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(emptyEventsJson);

        var command = new GenerateEventDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated empty or unparseable JSON response.");
        // 💡 Giải thích: Mảng 'events' rỗng được coi là phản hồi không hợp lệ vì không có dữ liệu sự kiện nào được tạo.
    }
}
