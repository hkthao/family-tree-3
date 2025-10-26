using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
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
    private readonly GenerateEventDataCommandHandler _handler;
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly Mock<IValidator<AIEventDto>> _mockAIEventDtoValidator;

    public GenerateEventDataCommandHandlerTests()
    {
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockChatProvider = _fixture.Freeze<Mock<IChatProvider>>();
        _mockAIEventDtoValidator = _fixture.Freeze<Mock<IValidator<AIEventDto>>>();
        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateEventDataCommandHandler(
            _mockChatProviderFactory.Object,
            _mockAIEventDtoValidator.Object,
            _context,
            _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // 🎯 Mục tiêu của test:
        //Xác minh rằng handler trả về một kết quả thất bại
        //khi AI trả về một phản hồi trống hoặc null.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockChatProvider để trả về một chuỗi trống.
        // 2. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("AI did not return a response.");

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống xử lý đúng trường hợp AI không tạo ra phản hồi,
        // ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        // 🎯 Mục tiêu của test:
        //Xác minh rằng handler trả về một kết quả thất bại
        //khi AI trả về một chuỗi JSON không hợp lệ.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
        // 2. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\" "); // Invalid JSON

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid JSON");

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống xử lý đúng trường hợp AI tạo ra JSON không hợp lệ,
        // ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsGenerated()
    {
        // 🎯 Mục tiêu của test:
        //Xác minh rằng handler trả về một danh sách trống
        //khi AI trả về JSON hợp lệ nhưng không có sự kiện nào được tạo.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockChatProvider để trả về JSON hợp lệ nhưng với danh sách sự kiện trống.
        // 2. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách sự kiện trả về là rỗng.

        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [] }"); // Valid JSON, empty events list

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống xử lý đúng trường hợp AI không tạo ra sự kiện nào,
        // trả về một danh sách trống thay vì lỗi.
    }

    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenFamilyNotFound()
    {
        // 🎯 Mục tiêu của test:
        //Xác minh rằng handler trả về các sự kiện với lỗi xác thực
        //khi AI tạo sự kiện cho một gia đình không tồn tại hoặc người dùng không có quyền quản lý.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile.
        // 3. Thiết lập _mockFamilyAuthorizationService để trả về lỗi "Family not found" khi AuthorizeFamilyAccess được gọi.
        // 4. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách sự kiện trả về có chứa lỗi xác thực cho FamilyName.

        // Arrange
        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"NonExistentFamily\" } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // It's a success in terms of AI response processing, but events have validation errors
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain("Family 'NonExistentFamily' not found or you do not have permission to manage it.");

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi AI tạo ra một sự kiện với FamilyName không hợp lệ,
        // hệ thống sẽ thêm lỗi xác thực vào sự kiện đó và trả về kết quả thành công
        // (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    }

    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenMultipleFamiliesFound()
    {
        // 🎯 Mục tiêu của test:
        //Xác minh rằng handler trả về các sự kiện với lỗi xác thực
        //khi AI tạo sự kiện cho một FamilyName/Code khớp với nhiều gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thêm vào DB.
        // 2. Tạo hai Family có cùng tên hoặc mã và thêm vào DB.
        // 3. Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName.
        // 4. Thiết lập _mockAuthorizationService để trả về UserProfile.
        // 5. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách sự kiện trả về có chứa lỗi xác thực cho FamilyName.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _context.UserProfiles.Add(userProfile);

        var familyName = "DuplicateFamily";
        var family1 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"DuplicateFamily\" } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain($"Multiple families found with name or code '{familyName}'. Please specify.");

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi AI tạo ra một sự kiện với FamilyName khớp với nhiều gia đình,
        // hệ thống sẽ thêm lỗi xác thực vào sự kiện đó và trả về kết quả thành công.
    }

    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenRelatedMemberNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các sự kiện với lỗi xác thực
        // khi AI tạo sự kiện với các thành viên liên quan không tìm thấy trong gia đình được chỉ định.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và một Family, sau đó thêm vào DB.
        // 2. Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName và RelatedMembers không tồn tại.
        // 3. Thiết lập _mockAuthorizationService để trả về UserProfile.
        // 4. Thiết lập _mockFamilyAuthorizationService để trả về thành công khi AuthorizeFamilyAccess được gọi.
        // 5. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách sự kiện trả về có chứa lỗi xác thực cho RelatedMembers.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _context.UserProfiles.Add(userProfile);

        var familyName = "TestFamily";
        var family = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"NonExistentMember\"] } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        var generatedEvents = result.Value!;
        generatedEvents.First().ValidationErrors.Should().Contain("Related member 'NonExistentMember' not found in family 'TestFamily'.");

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi AI tạo ra một sự kiện với RelatedMembers không tồn tại,
        // hệ thống sẽ thêm lỗi xác thực vào sự kiện đó và trả về kết quả thành công.
    }

    [Fact]
    public async Task Handle_ShouldReturnEventsSuccessfully_WhenValidAIResponse()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các sự kiện được tạo thành công
        // khi AI trả về một phản hồi hợp lệ và tất cả các thực thể liên quan được tìm thấy và ủy quyền.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập, Family và Member, sau đó thêm vào DB.
        // 2. Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName và RelatedMembers hợp lệ.
        // 3. Thiết lập _mockAuthorizationService để trả về UserProfile.
        // 4. Thiết lập _mockFamilyAuthorizationService để trả về thành công khi AuthorizeFamilyAccess được gọi.
        // 5. Thiết lập _mockAIEventDtoValidator để trả về ValidationResult thành công.
        // 6. Tạo một GenerateEventDataCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách sự kiện trả về có chứa sự kiện được tạo.
        // 3. Kiểm tra xem không có lỗi xác thực nào.

        // Arrange
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);

        var familyName = "TestFamily";
        var family = new Family { Id = Guid.NewGuid(), Name = familyName, Code = "TF1" };
        _context.Families.Add(family);

        var member = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "JD001" };
        _context.Members.Add(member);

        var familyUser = new FamilyUser { FamilyId = family.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Manager };
        _context.FamilyUsers.Add(familyUser);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Members.Should().HaveCount(1);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"startDate\": \"2023-01-01\", \"location\": \"Location 1\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"John Doe\"] } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);
        _mockUser.Setup(u => u.Roles).Returns([SystemRole.Admin.ToString()]);
        _mockAIEventDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        var generatedEvents = result.Value!;
        generatedEvents.First().Name.Should().Be("Event 1");
        generatedEvents.First().ValidationErrors.Should().BeEmpty();

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi AI tạo ra một sự kiện hợp lệ và tất cả các thực thể liên quan
        // được tìm thấy và ủy quyền, hệ thống sẽ trả về sự kiện đó mà không có lỗi xác thực.
    }
}
