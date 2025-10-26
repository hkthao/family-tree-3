using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Constants;
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

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi AI trả về một phản hồi trống hoặc null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi trống.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI không tạo ra phản hồi, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("AI did not return a response.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi AI trả về một chuỗi JSON không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI tạo ra JSON không hợp lệ, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\" "); // Invalid JSON

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI generated invalid response:");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách trống
    /// khi AI trả về JSON hợp lệ nhưng không có sự kiện nào được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ nhưng với danh sách sự kiện trống.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện trả về là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI không tạo ra sự kiện nào, trả về một danh sách trống thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsGenerated()
    {
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [] }"); // Valid JSON, empty events list

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện cho một gia đình không tồn tại hoặc người dùng không có quyền quản lý.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName không tồn tại.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho FamilyName.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với FamilyName không hợp lệ, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó và trả về kết quả thành công
    /// (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenFamilyNotFound()
    {
        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"NonExistentFamily\" } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // It's a success in terms of AI response processing, but events have validation errors
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain(string.Format(ErrorMessages.FamilyNotFound, "NonExistentFamily"));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện cho một FamilyName/Code khớp với nhiều gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo hai Family có cùng tên và thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ
    ///               với một sự kiện có FamilyName trùng lặp. Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho FamilyName.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với FamilyName khớp với nhiều gia đình, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó
    /// và trả về kết quả thành công (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenMultipleFamiliesFound()
    {
        var familyName = "DuplicateFamily";
        var family1 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"DuplicateFamily\" } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.MultipleFamiliesFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện với các thành viên liên quan không tìm thấy trong gia đình được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ
    ///               với một sự kiện có FamilyName và RelatedMembers không tồn tại.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho RelatedMembers.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với RelatedMembers không tồn tại, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó
    /// và trả về kết quả thành công (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenRelatedMemberNotFound()
    {
        var familyName = "TestFamily";
        var family = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"NonExistentMember\"] } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        var generatedEvents = result.Value!;
        generatedEvents.First().ValidationErrors.Should().Contain("Related member 'NonExistentMember' not found in family 'TestFamily'.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện được tạo thành công
    /// khi AI trả về một phản hồi hợp lệ và tất cả các thực thể liên quan được tìm thấy và ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile, Family và Member, sau đó thêm vào DB.
    ///               Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName và RelatedMembers hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanManageFamily trả về true.
    ///               Thiết lập _mockAIEventDtoValidator để trả về ValidationResult thành công.
    ///               Tạo một GenerateEventDataCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa sự kiện được tạo và không có lỗi xác thực nào.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện hợp lệ
    /// và tất cả các thực thể liên quan được tìm thấy và ủy quyền, hệ thống sẽ trả về sự kiện đó
    /// mà không có lỗi xác thực.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsSuccessfully_WhenValidAIResponse()
    {
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

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"startDate\": \"2023-01-01\", \"location\": \"Location 1\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"John Doe\"] } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id)).Returns(true);
        _mockAIEventDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult());

        var command = _fixture.Create<GenerateEventDataCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        var generatedEvents = result.Value!;
        generatedEvents.First().Name.Should().Be("Event 1");
        generatedEvents.First().ValidationErrors.Should().BeEmpty();
    }
}
