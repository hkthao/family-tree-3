using System.Text.Json;
using Application.NaturalLanguageInput.Commands.GenerateData;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.NaturalLanguageInput.Commands.GenerateData;

public class GenerateMemberDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly GenerateMemberDataCommandHandler _handler;

    public GenerateMemberDataCommandHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockChatProvider = new Mock<IChatProvider>();
        _fixture.Customize(new AutoMoqCustomization());

        _mockChatProviderFactory.Setup(f => f.GetProvider(ChatAIProvider.Local))
            .Returns(_mockChatProvider.Object);

        _handler = new GenerateMemberDataCommandHandler(
            _mockChatProviderFactory.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnMemberDataSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về dữ liệu thành viên thành công khi AI cung cấp JSON hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ chứa danh sách MemberDto.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa danh sách MemberDto mong đợi.
        var prompt = "Thêm thành viên tên Trần Văn A, sinh năm 1990.";
        var expectedMembers = new List<MemberDto>
        {
            new() { FirstName = "Trần Văn", LastName = "A", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1), Occupation = "Unknown" }
        };
        var aiResponseJson = JsonSerializer.Serialize(new { members = expectedMembers }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(aiResponseJson);

        var command = new GenerateMemberDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().FirstName.Should().Be("Trần Văn");
        // 💡 Giải thích: Handler phải phân tích cú pháp phản hồi JSON hợp lệ từ AI và trả về danh sách MemberDto.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi phản hồi từ AI là rỗng hoặc chỉ chứa khoảng trắng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về một chuỗi rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var prompt = "Generate some member data.";
        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(string.Empty);

        var command = new GenerateMemberDataCommand(prompt);

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
        var prompt = "Generate some member data.";
        var invalidJson = @"{ ""members"": [ { ""fullName"": ""Invalid JSON"", ""gender"": ""Male"" } "; // Malformed JSON

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(invalidJson);

        var command = new GenerateMemberDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid JSON");
        // 💡 Giải thích: Handler phải bắt lỗi JsonException khi phản hồi không phải là JSON hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseContainsEmptyMembersArray()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi phản hồi từ AI chứa một mảng 'members' rỗng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ nhưng với mảng 'members' rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var prompt = "Generate some member data.";
        var emptyMembersJson = "{ \"members\": [] }";

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(
                It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(emptyMembersJson);

        var command = new GenerateMemberDataCommand(prompt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated empty or unparseable JSON response.");
        // 💡 Giải thích: Mảng 'members' rỗng được coi là phản hồi không hợp lệ vì không có dữ liệu thành viên nào được tạo.
    }
}
