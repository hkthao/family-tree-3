using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Commands.GenerateMemberData;
using backend.Application.Members.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IValidator<AIMemberDto>> _mockAIMemberDtoValidator;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly GenerateMemberDataCommandHandler _handler;

    public GenerateMemberDataCommandHandlerTests()
    {
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockAIMemberDtoValidator = _fixture.Freeze<Mock<IValidator<AIMemberDto>>>();
        _mockChatProvider = new Mock<IChatProvider>();

        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateMemberDataCommandHandler(
            _mockChatProviderFactory.Object,
            _mockAIMemberDtoValidator.Object,
            _context,
            _mockAuthorizationService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesEmptyResponse()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi AI tạo ra phản hồi trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về chuỗi rỗng.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI did not return a response.");
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không tạo ra phản hồi.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesInvalidJson()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi AI tạo ra JSON không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về một chuỗi JSON không hợp lệ.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"members\": [ { \"firstName\": \"John\" "); // Invalid JSON

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid JSON");
        // 💡 Giải thích: Handler phải xử lý trường hợp AI tạo ra JSON không hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAIGeneratesNoMembers()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách trống khi AI tạo ra phản hồi JSON không có thành viên.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về JSON hợp lệ nhưng không có thành viên.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách thành viên trống.
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"members\": [] }"); // Valid JSON, but no members

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không tạo ra thành viên nào.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về JSON hợp lệ với một thành viên.
        //             Đảm bảo _context.Families không chứa gia đình đó.
        //             Mock FamilyAuthorizationService.AuthorizeFamilyAccess trả về thất bại.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, nhưng thành viên có lỗi xác thực về gia đình.
        var validJson = "{ \"members\": [ { \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"NonExistentFamily\" } ] }";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        // Ensure family is not found
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync();

        // Mock the validator to pass for other fields
        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        Assert.NotNull(result.Value); // Explicit null check
        result.Value.First().ValidationErrors.Should().Contain("Family 'NonExistentFamily' not found or you do not have permission to manage it.");
        // 💡 Giải thích: Handler phải báo cáo lỗi khi gia đình không tìm thấy hoặc người dùng không có quyền.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMultipleFamiliesFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi tìm thấy nhiều gia đình có cùng tên.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về JSON hợp lệ với một thành viên.
        //             Thêm nhiều gia đình có cùng tên vào DB.
        //             Mock FamilyAuthorizationService.AuthorizeFamilyAccess trả về thành công cho một gia đình.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, nhưng thành viên có lỗi xác thực về gia đình.
        var familyName = "DuplicateFamily";
        var validJson = $"{{ \"members\": [ {{ \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"{familyName}\" }} ] }}";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        // Add multiple families with the same name
        var family1 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family1);
        _context.Families.Add(family2);
        await _context.SaveChangesAsync();

        // Mock the validator to pass for other fields
        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        Assert.NotNull(result.Value); // Explicit null check
        result.Value.First()!.ValidationErrors.Should().Contain($"Multiple families found with name '{familyName}'. Please specify.");
        // 💡 Giải thích: Handler phải báo cáo lỗi khi tìm thấy nhiều gia đình có cùng tên.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIMemberDtoValidationFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler báo cáo lỗi khi xác thực AIMemberDto thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về JSON hợp lệ với một thành viên.
        //             Mock FamilyAuthorizationService.AuthorizeFamilyAccess trả về thành công.
        //             Mock IValidator<AIMemberDto>.ValidateAsync trả về kết quả xác thực thất bại.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, nhưng thành viên có lỗi xác thực.
        var validJson = "{ \"members\": [ { \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"ExistingFamily\" } ] }";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        var family = _fixture.Build<Family>().With(f => f.Name, "ExistingFamily").Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var validationErrors = new List<ValidationFailure>
        {
            new("FirstName", "First Name is required.")
        };
        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult(validationErrors));

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        Assert.NotNull(result.Value); // Explicit null check
        result.Value.First()!.ValidationErrors.Should().Contain("First Name is required.");
        // 💡 Giải thích: Handler phải báo cáo lỗi khi xác thực AIMemberDto thất bại.
    }

    [Fact]
    public async Task Handle_ShouldGenerateMemberDataSuccessfully_WithValidPrompt()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo dữ liệu thành viên thành công với lời nhắc hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock IChatProvider.GenerateResponseAsync trả về JSON hợp lệ với một thành viên.
        //             Thêm một gia đình vào DB. Mock FamilyAuthorizationService.AuthorizeFamilyAccess trả về thành công.
        //             Mock IValidator<AIMemberDto>.ValidateAsync trả về kết quả xác thực thành công.
        // 2. Act: Gọi phương thức Handle với một GenerateMemberDataCommand hợp lệ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách thành viên không rỗng.
        var validJson = "{ \"members\": [ { \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"ExistingFamily\" } ] }";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        var family = _fixture.Build<Family>().With(f => f.Name, "ExistingFamily").Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult()); // Validation passes

        var command = _fixture.Build<GenerateMemberDataCommand>()
            .With(c => c.Prompt, "Generate data for John Doe in ExistingFamily.")
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value!.First()!.FirstName.Should().Be("John");
        result.Value!.First()!.LastName.Should().Be("Doe");
        result.Value!.First()!.FamilyName.Should().Be("ExistingFamily");
        // 💡 Giải thích: Handler phải tạo dữ liệu thành viên thành công khi tất cả các điều kiện đều hợp lệ.
    }
}
