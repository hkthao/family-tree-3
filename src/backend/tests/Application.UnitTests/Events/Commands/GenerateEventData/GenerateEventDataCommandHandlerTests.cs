
using System.Text.Json;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added this line
using backend.Application.Events.Commands.GenerateEventData;
using backend.Application.Events.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;
    private readonly Mock<IValidator<AIEventDto>> _aiEventDtoValidatorMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GenerateEventDataCommandHandler _handler;

    public GenerateEventDataCommandHandlerTests()
    {
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();
        _aiEventDtoValidatorMock = new Mock<IValidator<AIEventDto>>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);

        _handler = new GenerateEventDataCommandHandler(
            _chatProviderFactoryMock.Object,
            _aiEventDtoValidatorMock.Object,
            _context,
            _authorizationServiceMock.Object);
    }

    /// <summary>
    /// Kiểm tra trường hợp thành công: tạo dữ liệu sự kiện khi lệnh và phản hồi AI hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateEventData_WhenValidCommandAndAIResponseIsValid()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        var member = new Member("Doe", "John", "JD001", familyId) { Id = memberId };
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var aiResponseJson = "{\"events\":[{\"name\":\"Test Event\",\"type\":\"Other\",\"startDate\":\"2023-01-01\",\"location\":\"Test Location\",\"familyName\":\"Test Family\",\"relatedMembers\":[\"John Doe\"]}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

        var command = new GenerateEventDataCommand("Generate a test event.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var eventDto = result.Value!.First();
        eventDto.Name.Should().Be("Test Event");
        eventDto.FamilyId.Should().Be(familyId);
        eventDto.ValidationErrors.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trả về thất bại khi phản hồi từ AI rỗng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // Arrange
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(" ");
        var command = new GenerateEventDataCommand("Generate empty response.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.NoAIResponse);
    }

    /// <summary>
    /// Kiểm tra trả về danh sách rỗng khi AI không tạo ra sự kiện nào.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAIResponseContainsNoEvents()
    {
        // Arrange
        var aiResponseJson = "{\"events\":[]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        var command = new GenerateEventDataCommand("Generate no events.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trả về thất bại khi phản hồi AI là JSON không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync("invalid json");
        var command = new GenerateEventDataCommand("Generate invalid json.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().StartWith(string.Format(ErrorMessages.InvalidAIResponse, ""));
    }

    /// <summary>
    /// Kiểm tra thêm lỗi xác thực khi không tìm thấy FamilyName.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenFamilyNameIsNotFound()
    {
        // Arrange
        var aiResponseJson = "{\"events\":[{\"familyName\":\"NonExistentFamily\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(string.Format(ErrorMessages.FamilyNotFound, "NonExistentFamily"));
    }
    
    /// <summary>
    /// Kiểm tra thêm lỗi xác thực khi tìm thấy nhiều gia đình trùng tên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenMultipleFamiliesFound()
    {
        // Arrange
        _context.Families.Add(new Family { Name = "Duplicate Family", Code = "DF1" });
        _context.Families.Add(new Family { Name = "Duplicate Family", Code = "DF2" });
        await _context.SaveChangesAsync();

        var aiResponseJson = "{\"events\":[{\"familyName\":\"Duplicate Family\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.MultipleFamiliesFound);
    }

    /// <summary>
    /// Kiểm tra thêm lỗi xác thực khi người dùng không có quyền truy cập gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenFamilyIsNotAccessible()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Inaccessible Family", Code = "IF1" });
        await _context.SaveChangesAsync();

        var aiResponseJson = "{\"events\":[{\"familyName\":\"Inaccessible Family\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false); // No access
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.AccessDenied);
        result.Value!.First().FamilyId.Should().BeNull();
    }

    /// <summary>
    /// Kiểm tra thêm lỗi xác thực khi không tìm thấy thành viên liên quan.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenRelatedMemberIsNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TF1" });
        await _context.SaveChangesAsync();

        var aiResponseJson = "{\"events\":[{\"familyName\":\"Test Family\", \"relatedMembers\":[\"NonExistent Member\"]}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(string.Format(ErrorMessages.NotFound, "Related member 'NonExistent Member' in family 'Test Family'"));
    }

    /// <summary>
    /// Kiểm tra thêm lỗi xác thực khi AIEventDto không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldAddValidationError_WhenAIEventDtoValidationFails()
    {
        // Arrange
        var validationError = "Name is required.";
        var aiResponseJson = "{\"events\":[{\"name\":\"\"}]}"; // Empty name
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", validationError) }));
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().ValidationErrors.Should().Contain(validationError);
    }
    
    /// <summary>
    /// Kiểm tra loại sự kiện được mặc định là 'Other' nếu thiếu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDefaultEventTypeToOther_WhenTypeIsMissing()
    {
        // Arrange
        var aiResponseJson = "{\"events\":[{\"name\":\"Event without type\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);
        _aiEventDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
        var command = new GenerateEventDataCommand("Test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.First().Type.Should().Be(EventType.Other.ToString());
    }

    /// <summary>
    /// Kiểm tra trả về thất bại khi có lỗi không mong muốn xảy ra.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var exceptionMessage = "Unexpected error.";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ThrowsAsync(new Exception(exceptionMessage));
        var command = new GenerateEventDataCommand("Test");
        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
    }
}