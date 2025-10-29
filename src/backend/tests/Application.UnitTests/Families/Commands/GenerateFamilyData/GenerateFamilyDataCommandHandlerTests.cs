using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly GenerateFamilyDataCommandHandler _handler;
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly Mock<IValidator<FamilyDto>> _mockFamilyDtoValidator;

    public GenerateFamilyDataCommandHandlerTests()
    {
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockChatProvider = _fixture.Freeze<Mock<IChatProvider>>();
        _mockFamilyDtoValidator = _fixture.Freeze<Mock<IValidator<FamilyDto>>>();

        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateFamilyDataCommandHandler(
            _mockChatProviderFactory.Object,
            _mockFamilyDtoValidator.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo dữ liệu gia đình thành công
    /// khi AI trả về JSON hợp lệ và dữ liệu vượt qua xác thực.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ chứa dữ liệu gia đình.
    ///               Thiết lập _mockFamilyDtoValidator để trả về kết quả xác thực thành công.
    ///               Tạo một GenerateFamilyDataCommand với một prompt bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa danh sách FamilyDto.
    ///              Kiểm tra xem các FamilyDto có dữ liệu mong đợi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng luồng thành công
    /// từ việc tạo phản hồi AI đến việc phân tích cú pháp và xác thực dữ liệu hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateFamilyDataSuccessfully_WhenAIReturnsValidJson()
    {
        // Arrange
        var prompt = "Tạo một gia đình tên Nguyễn ở Hà Nội.";
        var aiResponseJson = "{\"families\": [{\"name\": \"Gia đình Nguyễn\", \"address\": \"Hà Nội\", \"visibility\": \"Public\"}]}";

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                           .ReturnsAsync(aiResponseJson);

        _mockFamilyDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var command = new GenerateFamilyDataCommand(prompt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be("Gia đình Nguyễn");
        result.Value!.First().Address.Should().Be("Hà Nội");
        result.Value!.First().Visibility.Should().Be("Public");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi phản hồi từ AI là rỗng hoặc khoảng trắng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi rỗng.
    ///               Tạo một GenerateFamilyDataCommand với một prompt bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và chứa thông báo lỗi ErrorMessages.NoAIResponse.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu AI không tạo ra phản hồi,
    /// hệ thống nên báo cáo lỗi để người dùng biết rằng không có dữ liệu nào được tạo.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // Arrange
        var prompt = "Generate an empty response.";
        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                           .ReturnsAsync(string.Empty);

        var command = new GenerateFamilyDataCommand(prompt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.NoAIResponse);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi phản hồi từ AI là JSON không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
    ///               Tạo một GenerateFamilyDataCommand với một prompt bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và chứa thông báo lỗi ErrorMessages.InvalidAIResponse.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu AI trả về JSON không thể phân tích cú pháp,
    /// hệ thống nên báo cáo lỗi để người dùng biết rằng dữ liệu không thể được xử lý.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        var prompt = "Generate invalid json.";
        var invalidJson = "{\"families\": [{\"name\": \"Gia đình Nguyễn\", \"address\": \"Hà Nội\", \"visibility\": \"Public\"}]" + "invalid"; // Invalid JSON

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                           .ReturnsAsync(invalidJson);

        var command = new GenerateFamilyDataCommand(prompt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid response");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dữ liệu FamilyDto được tạo bởi AI không vượt qua xác thực.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ.
    ///               Thiết lập _mockFamilyDtoValidator để trả về kết quả xác thực thất bại với một thông báo lỗi.
    ///               Tạo một GenerateFamilyDataCommand với một prompt bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (vì lỗi xác thực được ghi vào FamilyDto).
    ///              Kiểm tra xem FamilyDto trong kết quả có chứa thông báo lỗi xác thực.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Ngay cả khi AI tạo ra JSON hợp lệ,
    /// dữ liệu vẫn cần phải vượt qua các quy tắc nghiệp vụ. Test này đảm bảo rằng các lỗi xác thực
    /// được ghi lại và trả về một cách chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDtoValidationFails()
    {
        // Arrange
        var prompt = "Tạo một gia đình với tên không hợp lệ.";
        var aiResponseJson = "{\"families\": [{\"name\": \"\", \"address\": \"Hà Nội\", \"visibility\": \"Public\"}]}";

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                           .ReturnsAsync(aiResponseJson);

        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Name", "Name is required."));
        _mockFamilyDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(validationResult);

        var command = new GenerateFamilyDataCommand(prompt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Validation errors are part of the success result
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain("Name is required.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách rỗng
    /// khi phản hồi từ AI không chứa bất kỳ gia đình nào.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON hợp lệ nhưng không có gia đình.
    ///               Tạo một GenerateFamilyDataCommand với một prompt bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa một danh sách FamilyDto rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu AI không tạo ra bất kỳ gia đình nào,
    /// hệ thống nên trả về một danh sách rỗng thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAIResponseContainsNoFamilies()
    {
        // Arrange
        var prompt = "Generate no families.";
        var aiResponseJson = "{\"families\": []}";

        _mockChatProvider.Setup(cp => cp.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                           .ReturnsAsync(aiResponseJson);

        var command = new GenerateFamilyDataCommand(prompt);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
