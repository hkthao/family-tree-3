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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dịch vụ AI tạo ra một phản hồi trống hoặc chỉ chứa khoảng trắng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi rỗng.
    ///               Tạo một GenerateMemberDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp
    ///              (ErrorMessages.NoContent) và ErrorSource là ErrorSources.NoContent.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp dịch vụ AI không thể tạo ra nội dung phản hồi, ngăn chặn việc xử lý tiếp
    /// với dữ liệu không hợp lệ hoặc thiếu sót.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesEmptyResponse()
    {
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.NoAIResponse);
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.NoContent);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dịch vụ AI tạo ra một phản hồi JSON không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON không hợp lệ.
    ///               Tạo một GenerateMemberDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp
    ///              (ErrorMessages.InvalidJson) và ErrorSource là ErrorSources.Serialization.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp dịch vụ AI trả về dữ liệu không thể phân tích cú pháp JSON, ngăn chặn
    /// các lỗi trong quá trình deserialization và đảm bảo tính ổn định của ứng dụng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesInvalidJson()
    {
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"members\": [ { \"firstName\": \"John\" "); // Invalid JSON

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.InvalidAIResponse, "Invalid JSON format"));
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.Exception);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách rỗng các AIMemberDto
    /// khi dịch vụ AI tạo ra một phản hồi JSON hợp lệ nhưng không chứa bất kỳ thành viên nào.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON hợp lệ
    ///               nhưng với một mảng "members" rỗng.
    ///               Tạo một GenerateMemberDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách Value là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp dịch vụ AI không tạo ra bất kỳ dữ liệu thành viên nào, trả về một danh sách rỗng
    /// thay vì lỗi, cho phép ứng dụng tiếp tục hoạt động mà không bị gián đoạn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAIGeneratesNoMembers()
    {
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"members\": [] }");

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler báo cáo lỗi validation cho thành viên
    /// khi FamilyName được cung cấp trong AIMemberDto không tìm thấy trong hệ thống
    /// hoặc người dùng không có quyền quản lý gia đình đó.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON hợp lệ
    ///               chứa một AIMemberDto với một FamilyName không tồn tại.
    ///               Đảm bảo rằng gia đình với FamilyName đó không có trong context.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về ValidationResult thành công cho các trường khác.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) nhưng danh sách Value
    ///              chứa AIMemberDto có thuộc tính ValidationErrors không rỗng và chứa thông báo lỗi
    ///              về việc không tìm thấy gia đình hoặc không có quyền.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống kiểm tra
    /// tính hợp lệ của FamilyName được cung cấp bởi AI và quyền của người dùng đối với gia đình đó.
    /// Nếu không tìm thấy gia đình hoặc người dùng không có quyền, lỗi sẽ được ghi nhận vào
    /// ValidationErrors của AIMemberDto tương ứng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        var validJson = "{ \"members\": [ { \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"NonExistentFamily\" } ] }";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        Assert.NotNull(result.Value);
        result.Value.First().ValidationErrors.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.FamilyNotFound, "NonExistentFamily"));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler báo cáo lỗi validation cho thành viên
    /// khi có nhiều hơn một gia đình với cùng FamilyName được tìm thấy trong hệ thống.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON hợp lệ
    ///               chứa một AIMemberDto với một FamilyName cụ thể.
    ///               Thêm nhiều gia đình với cùng FamilyName đó vào context.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về ValidationResult thành công cho các trường khác.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) nhưng danh sách Value
    ///              chứa AIMemberDto có thuộc tính ValidationErrors không rỗng và chứa thông báo lỗi
    ///              về việc tìm thấy nhiều gia đình.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp không thể xác định duy nhất một gia đình dựa trên FamilyName, yêu cầu người dùng
    /// cung cấp thông tin rõ ràng hơn để tránh nhầm lẫn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMultipleFamiliesFound()
    {
        var familyName = "DuplicateFamily";
        var validJson = $"{{ \"members\": [ {{ \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"{familyName}\" }} ] }}";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        var family1 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family1);
        _context.Families.Add(family2);
        await _context.SaveChangesAsync();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateMemberDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        Assert.NotNull(result.Value);
        result.Value.First()!.ValidationErrors.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.MultipleFamiliesFound, familyName));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler báo cáo lỗi validation cho thành viên
    /// khi AIMemberDto được tạo bởi AI không vượt qua các quy tắc validation nghiệp vụ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON hợp lệ
    ///               chứa một AIMemberDto.
    ///               Thêm một gia đình vào context để FamilyName hợp lệ.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về một ValidationResult chứa lỗi
    ///               cho AIMemberDto.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công (IsSuccess = true) nhưng danh sách Value
    ///              chứa AIMemberDto có thuộc tính ValidationErrors không rỗng và chứa thông báo lỗi
    ///              từ validator.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống áp dụng
    /// các quy tắc validation nghiệp vụ cho dữ liệu thành viên được tạo bởi AI. Nếu dữ liệu
    /// không hợp lệ, lỗi sẽ được ghi nhận vào ValidationErrors của AIMemberDto tương ứng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIMemberDtoValidationFails()
    {
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
        Assert.NotNull(result.Value);
        result.Value.First()!.ValidationErrors.Should().Contain("First Name is required.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo dữ liệu thành viên thành công
    /// khi được cung cấp một lời nhắc hợp lệ và tất cả các điều kiện khác đều hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi JSON hợp lệ
    ///               chứa một AIMemberDto.
    ///               Thêm một gia đình vào context với FamilyName khớp với AIMemberDto.
    ///               Thiết lập _mockAIMemberDtoValidator để trả về ValidationResult thành công.
    ///               Tạo một GenerateMemberDataCommand với một Prompt hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách Value chứa AIMemberDto
    ///              với các thuộc tính được điền chính xác từ phản hồi của AI.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể
    /// tương tác thành công với dịch vụ AI để tạo dữ liệu thành viên, xử lý phản hồi JSON,
    /// và trả về dữ liệu thành viên đã được tạo một cách chính xác khi tất cả các điều kiện đều hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateMemberDataSuccessfully_WithValidPrompt()
    {
        var validJson = "{ \"members\": [ { \"firstName\": \"John\", \"lastName\": \"Doe\", \"familyName\": \"ExistingFamily\" } ] }";
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(validJson);

        var family = _fixture.Build<Family>().With(f => f.Name, "ExistingFamily").Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _mockAIMemberDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new ValidationResult());

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
    }
}
