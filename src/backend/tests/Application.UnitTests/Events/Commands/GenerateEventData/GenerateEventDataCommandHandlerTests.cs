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
            _mockAuthorizationService.Object
        );
            
        _mockAuthorizationService.Setup(s => s.CanAccessFamily(It.IsAny<Guid>())).Returns(true);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi AI trả về một phản hồi trống hoặc null.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi trống.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI không tạo ra phản hồi, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsEmpty()
    {
        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);
        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.NoAIResponse);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi AI trả về một chuỗi JSON không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về một chuỗi JSON không hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI tạo ra JSON không hợp lệ, ngăn chặn lỗi và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\" "); // Invalid JSON
        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.InvalidAIResponse.Split('{')[0]);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách trống
    /// khi AI trả về JSON hợp lệ nhưng không có sự kiện nào được tạo.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ nhưng với danh sách sự kiện trống.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện trả về là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp AI không tạo ra sự kiện nào, trả về một danh sách trống thay vì lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEventsGenerated()
    {
        // Arrange
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync("{ \"events\": [] }");
        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện cho một gia đình mà người dùng hiện tại không có quyền truy cập.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ
    ///               với một sự kiện có FamilyName hợp lệ. Thiết lập _mockAuthorizationService để CanAccessFamily trả về false.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực về quyền truy cập gia đình.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống kiểm tra quyền truy cập
    /// của người dùng đối với gia đình được đề cập trong sự kiện do AI tạo ra và thêm lỗi xác thực
    /// nếu người dùng không có quyền, nhưng vẫn trả về kết quả thành công vì quá trình AI đã hoàn tất.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenUserCannotAccessFamily()
    {
        // Arrange
        var familyName = "AccessibleFamily";
        var family = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"AccessibleFamily\" } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        _mockAuthorizationService.Setup(s => s.CanAccessFamily(family.Id)).Returns(false);

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.AccessDenied);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện cho một gia đình không tồn tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho FamilyName.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với FamilyName không hợp lệ, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó và trả về kết quả thành công
    /// (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenFamilyNotFound()
    {
        // Arrange
        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"NonExistentFamily\" } ] }";
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
        result.Value!.First().ValidationErrors.Should().Contain(string.Format(ErrorMessages.FamilyNotFound, "NonExistentFamily"));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện cho một FamilyName/Code khớp với nhiều gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo hai Family có cùng tên và thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ
    ///               với một sự kiện có FamilyName trùng lặp.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho FamilyName.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với FamilyName khớp với nhiều gia đình, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó
    /// và trả về kết quả thành công (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenMultipleFamiliesFound()
    {
        // Arrange
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
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.MultipleFamiliesFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện với một định danh thành viên liên quan khớp với nhiều thành viên
    /// trong gia đình được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và hai Member có cùng tên/mã trong Family đó, sau đó thêm vào DB.
    ///               Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName
    ///               và một định danh RelatedMember trùng lặp. Thiết lập _mockAuthorizationService để CanAccessFamily trả về true.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực về việc tìm thấy nhiều thành viên.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp khi một định danh thành viên liên quan không đủ duy nhất để xác định một thành viên cụ thể,
    /// ngăn chặn việc gán sai và cung cấp thông báo lỗi rõ ràng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenMultipleRelatedMembersFound()
    {
        // Arrange
        var familyName = "TestFamily";
        var family = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family);

        var memberIdentifier = "John Doe";
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "JD001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "JD002" };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"" + memberIdentifier + "\"] } ] }";
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
        result.Value!.First().ValidationErrors.Should().Contain(ErrorMessages.MultipleMembersFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AI tạo sự kiện với các thành viên liên quan không tìm thấy trong gia đình được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ
    ///               với một sự kiện có FamilyName và RelatedMembers không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực cho RelatedMembers.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện
    /// với RelatedMembers không tồn tại, hệ thống sẽ thêm lỗi xác thực vào sự kiện đó
    /// và trả về kết quả thành công (vì quá trình xử lý AI thành công, nhưng dữ liệu sự kiện có lỗi).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenRelatedMemberNotFound()
    {
        // Arrange
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
        generatedEvents.First().ValidationErrors.Should().Contain(string.Format(ErrorMessages.NotFound, "Related member 'NonExistentMember' in family 'TestFamily'"));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện với lỗi xác thực
    /// khi AIEventDtoValidator phát hiện lỗi trong dữ liệu sự kiện do AI tạo ra.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và Member, sau đó thêm vào DB. Thiết lập _mockChatProvider để trả về JSON hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanAccessFamily trả về true.
    ///               Thiết lập _mockAIEventDtoValidator để trả về ValidationResult với lỗi.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa lỗi xác thực từ validator.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng các lỗi validation
    /// được phát hiện bởi AIEventDtoValidator được thu thập và trả về cùng với các sự kiện,
    /// cho phép xử lý lỗi chi tiết hơn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsWithValidationErrors_WhenAIEventDtoValidationFails()
    {
        // Arrange
        var familyName = "TestFamily";
        var family = _fixture.Build<Family>().With(f => f.Name, familyName).Create();
        _context.Families.Add(family);

        var member = _fixture.Build<Member>().With(m => m.FamilyId, family.Id).With(m => m.FirstName, "John").With(m => m.LastName, "Doe").Create();
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var aiResponseJson = "{ \"events\": [ { \"name\": \"Event 1\", \"type\": \"Other\", \"familyName\": \"TestFamily\", \"relatedMembers\": [\"" + member.FirstName + " " + member.LastName + "\"] } ] }";
        _mockChatProvider.Setup(p => p.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(aiResponseJson);

        var validationFailure = new ValidationFailure("Name", "Event name is too short.");
        _mockAIEventDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<AIEventDto>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { validationFailure }));

        var command = _fixture.Create<GenerateEventDataCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().ValidationErrors.Should().Contain("Event name is too short.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về các sự kiện được tạo thành công
    /// khi AI trả về một phản hồi hợp lệ và tất cả các thực thể liên quan được tìm thấy và ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile, Family và Member, sau đó thêm vào DB.
    ///               Thiết lập _mockChatProvider để trả về JSON hợp lệ với một sự kiện có FamilyName và RelatedMembers hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanManageFamily trả về true.
    ///               Thiết lập _mockAIEventDtoValidator để trả về ValidationResult thành công.
    ///    - Act: Gọi phương thức Handle của handler với một GenerateEventDataCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem danh sách sự kiện trả về
    ///              có chứa sự kiện được tạo và không có lỗi xác thực nào.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi AI tạo ra một sự kiện hợp lệ
    /// và tất cả các thực thể liên quan được tìm thấy và ủy quyền, hệ thống sẽ trả về sự kiện đó
    /// mà không có lỗi xác thực.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEventsSuccessfully_WhenValidAIResponse()
    {
        // Arrange
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User", FirstName = "Test", LastName = "User", Phone = "1234567890" };
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
    }
}