using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.GenerateRelationshipData;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.GenerateRelationshipData;

public class GenerateRelationshipDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly Mock<IValidator<AIRelationshipDto>> _mockAIRelationshipDtoValidator;
    private readonly Mock<ILogger<GenerateRelationshipDataCommandHandler>> _mockLogger;
    private readonly GenerateRelationshipDataCommandHandler _handler;

    public GenerateRelationshipDataCommandHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>();
        _mockChatProvider = new Mock<IChatProvider>();
        _mockAIRelationshipDtoValidator = new Mock<IValidator<AIRelationshipDto>>();
        _mockLogger = new Mock<ILogger<GenerateRelationshipDataCommandHandler>>();
        _fixture.Customize(new AutoMoqCustomization());

        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
            .Returns(_mockChatProvider.Object);

        _handler = new GenerateRelationshipDataCommandHandler(
            _mockChatProviderFactory.Object,
            _mockAIRelationshipDtoValidator.Object,
            _context,
            _mockAuthorizationService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi AI không trả về phản hồi.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về một chuỗi rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync(string.Empty);

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI did not return a response.");
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không tạo ra phản hồi.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenAIResponseIsInvalidJson()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi AI trả về JSON không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về một chuỗi JSON không hợp lệ.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync("{invalid json");

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid response");
        // 💡 Giải thích: Handler phải xử lý lỗi khi JSON từ AI không thể deserialize.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoRelationshipsGeneratedByAI()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi AI không tạo ra mối quan hệ nào (JSON hợp lệ nhưng danh sách Relationships rỗng).
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về JSON hợp lệ nhưng với danh sách Relationships rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa danh sách rỗng.
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync("{ \"relationships\": [] }");

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không tìm thấy mối quan hệ nào để tạo.
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipsWithValidationErrorsWhenMembersNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các mối quan hệ với lỗi validation khi thành viên nguồn hoặc đích không tìm thấy.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về JSON với SourceMemberName và/hoặc TargetMemberName không tồn tại trong _context.Members.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công nhưng các AIRelationshipDto có lỗi validation phù hợp.
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync("{ \"relationships\": [ { \"sourceMemberName\": \"Non Existent Source\", \"targetMemberName\": \"Non Existent Target\", \"type\": \"Father\" } ] }");

        _context.Members.RemoveRange(_context.Members); // Clear members to ensure not found
        await _context.SaveChangesAsync();

        _mockAIRelationshipDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // No FluentValidation errors

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().NotBeEmpty();
        result.Value!.First().ValidationErrors.Should().Contain(e => e.Contains("Source member 'Non Existent Source' not found."));
        result.Value!.First().ValidationErrors.Should().Contain(e => e.Contains("Target member 'Non Existent Target' not found."));
        // 💡 Giải thích: Handler phải xác định và báo cáo lỗi khi không tìm thấy thành viên.
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipsWithValidationErrorsWhenAuthorizationFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các mối quan hệ với lỗi validation khi ủy quyền truy cập gia đình thất bại.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về JSON với SourceMemberName và TargetMemberName tồn tại. Thiết lập _mockAuthorizationService.CanAccessFamily trả về false.
        var familyId = Guid.NewGuid();
        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Source", LastName = "Existent", Code = "SM001" };
        var targetMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Target", LastName = "Existent", Code = "TM001" };
        _context.Members.AddRange(sourceMember, targetMember);
        await _context.SaveChangesAsync();

        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync($"{{ \"relationships\": [ {{ \"sourceMemberName\": \"Existent Source\", \"targetMemberName\": \"Existent Target\", \"type\": \"Father\" }} ] }}");

        _mockAuthorizationService.Setup(s => s.CanAccessFamily(It.IsAny<Guid>()))
            .Returns(false);

        _mockAIRelationshipDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().NotBeEmpty();
        result.Value!.First().ValidationErrors.Should().Contain(e => e.Contains("Access denied."));
        // 💡 Giải thích: Handler phải xác định và báo cáo lỗi khi người dùng không có quyền truy cập gia đình của thành viên.
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipsWithValidationErrorsWhenAIRelationshipDtoValidationFails()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các mối quan hệ với lỗi validation khi AIRelationshipDto không hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về JSON với AIRelationshipDto không hợp lệ (ví dụ: Type không hợp lệ). Thiết lập _mockAIRelationshipDtoValidator.ValidateAsync trả về ValidationResult với lỗi.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công nhưng các AIRelationshipDto có lỗi validation phù hợp.
        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = Guid.NewGuid(), FirstName = "Valid", LastName = "Source", Code = "SM001" };
        var targetMember = new Member { Id = Guid.NewGuid(), FamilyId = Guid.NewGuid(), FirstName = "Valid", LastName = "Target", Code = "TM001" };
        _context.Members.AddRange(sourceMember, targetMember);
        await _context.SaveChangesAsync();

        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync("{ \"relationships\": [ { \"sourceMemberName\": \"Valid Source\", \"targetMemberName\": \"Valid Target\", \"type\": \"Father\" } ] }");

        _mockAuthorizationService.Setup(s => s.CanAccessFamily(It.IsAny<Guid>()))
            .Returns(true);

        var validationFailures = new List<ValidationFailure>
        {
            new("Type", "Loại mối quan hệ không hợp lệ.")
        };
        _mockAIRelationshipDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().NotBeEmpty();
        result.Value!.First().ValidationErrors.Should().Contain(e => e.Contains("Loại mối quan hệ không hợp lệ."));
        // 💡 Giải thích: Handler phải tổng hợp lỗi từ validator của AIRelationshipDto.
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessfullyGeneratedRelationships()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các mối quan hệ được tạo thành công khi tất cả các bước đều hợp lệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockChatProvider.GenerateResponseAsync trả về JSON hợp lệ. Thiết lập _context.Members để tìm thấy thành viên. Thiết lập _mockFamilyAuthorizationService.AuthorizeFamilyAccess trả về Result<Family>.Success. Thiết lập _mockAIRelationshipDtoValidator.ValidateAsync trả về ValidationResult hợp lệ.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa danh sách các AIRelationshipDto hợp lệ.
        var familyId = Guid.NewGuid();
        var sourceMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Source", LastName = "Valid", Code = "SM001" };
        var targetMember = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Target", LastName = "Valid", Code = "TM001" };
        _context.Members.AddRange(sourceMember, targetMember);
        await _context.SaveChangesAsync();

        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
            .ReturnsAsync($"{{ \"relationships\": [ {{ \"sourceMemberName\": \"Valid Source\", \"targetMemberName\": \"Valid Target\", \"type\": \"Father\", \"order\": 1 }} ] }}");

        _mockAuthorizationService.Setup(s => s.CanAccessFamily(It.IsAny<Guid>()))
            .Returns(true);

        _mockAIRelationshipDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIRelationshipDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var command = new GenerateRelationshipDataCommand("Some prompt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.First().SourceMemberId.Should().Be(sourceMember.Id);
        result.Value!.First().TargetMemberId.Should().Be(targetMember.Id);
        result.Value!.First().Type.Should().Be(RelationshipType.Father);
        result.Value!.First().ValidationErrors.Should().BeEmpty();
        // 💡 Giải thích: Handler phải xử lý thành công toàn bộ quy trình.
    }
}
