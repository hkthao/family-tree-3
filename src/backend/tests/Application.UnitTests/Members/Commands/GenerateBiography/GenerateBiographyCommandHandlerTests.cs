using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Commands.GenerateBiography;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandHandlerTests : TestBase
{
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly GenerateBiographyCommandHandler _handler;

    public GenerateBiographyCommandHandlerTests()
    {
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockChatProvider = new Mock<IChatProvider>();
        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateBiographyCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockChatProviderFactory.Object
        );
    }



    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy thành viên được chỉ định để tạo tiểu sử.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id trả về một giá trị hợp lệ. Đảm bảo không có thành viên nào
    ///               trong context khớp với MemberId trong command.
    ///               Tạo một GenerateBiographyCommand với một MemberId không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp
    ///              (ErrorMessages.NotFound) và ErrorSource là ErrorSources.NotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể tạo
    /// tiểu sử cho một thành viên không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());

        var nonExistentMemberId = Guid.NewGuid();
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, nonExistentMemberId)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.NotFound, $"Member with ID {nonExistentMemberId}"));
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không được ủy quyền để truy cập vào gia đình của thành viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một thành viên và thêm vào context. Thiết lập _mockUser.Id trả về một giá trị hợp lệ.
    ///               Thiết lập _mockAuthorizationService để IsAdmin trả về false và CanAccessFamily trả về false
    ///               cho FamilyId của thành viên.
    ///    - Act: Gọi phương thức Handle với GenerateBiographyCommand cho thành viên đó.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp
    ///              (ErrorMessages.AccessDenied) và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng
    /// có quyền truy cập vào gia đình mới có thể tạo tiểu sử cho thành viên trong gia đình đó,
    /// bảo vệ dữ liệu gia đình khỏi truy cập trái phép.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthorized()
    {
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(member.FamilyId)).Returns(false);

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi dịch vụ AI tạo ra một tiểu sử trống hoặc chỉ chứa khoảng trắng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một thành viên và thêm vào context. Thiết lập _mockUser.Id trả về một giá trị hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanAccessFamily trả về true (hoặc IsAdmin = true).
    ///               Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi rỗng.
    ///               Tạo một GenerateBiographyCommand cho thành viên đó.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp
    ///              (ErrorMessages.NoContent) và ErrorSource là ErrorSources.NoContent.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// trường hợp dịch vụ AI không thể tạo ra nội dung tiểu sử, ngăn chặn việc lưu trữ
    /// các tiểu sử rỗng hoặc không có ý nghĩa.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesEmptyBiography()
    {
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(member.FamilyId)).Returns(true); // Assume authorized
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty);

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.NoAIResponse);
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.NoContent);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo tiểu sử thành công
    /// khi được cung cấp dữ liệu hệ thống và một tông giọng cụ thể.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và một Member, thêm vào context. Thiết lập _mockUser.Id trả về một giá trị hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanAccessFamily trả về true (hoặc IsAdmin = true).
    ///               Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi tiểu sử hợp lệ.
    ///               Tạo một GenerateBiographyCommand với UseSystemData = true và một Tone cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value chứa nội dung tiểu sử không rỗng.
    ///              Kiểm tra các thông điệp được gửi đến ChatProvider để đảm bảo chúng chứa dữ liệu hệ thống
    ///              và yêu cầu tông giọng chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể tương tác
    /// với dịch vụ AI để tạo tiểu sử dựa trên dữ liệu có sẵn và các yêu cầu về tông giọng, đồng thời
    /// trả về kết quả thành công khi quá trình này diễn ra đúng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateBiographySuccessfully_WithSystemDataAndSpecificTone()
    {
        var family = _fixture.Create<Family>();
        var member = _fixture.Build<Member>()
            .With(m => m.FamilyId, family.Id)
            .With(m => m.Family, family)
            .With(m => m.FirstName, "John")
            .With(m => m.LastName, "Doe")
            .With(m => m.Gender, "Male")
            .With(m => m.DateOfBirth, new DateTime(1950, 1, 1))
            .With(m => m.PlaceOfBirth, "New York")
            .With(m => m.Occupation, "Engineer")
            .Create();
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(member.FamilyId)).Returns(true); // Assume authorized

        List<ChatMessage>? capturedMessages = null;
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .Callback<List<ChatMessage>>(messages => capturedMessages = messages)
                         .ReturnsAsync("This is a generated biography with system data and a specific tone.");

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .With(c => c.UseSystemData, true)
            .With(c => c.Tone, BiographyTone.Historical)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().NotBeEmpty();
        result.Value.Content.Should().Contain("This is a generated biography");

        capturedMessages.Should().NotBeNull();
        if (capturedMessages == null) throw new Xunit.Sdk.XunitException("capturedMessages should not be null.");
        capturedMessages.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("historical and factual tone");
        capturedMessages[1].Role.Should().Be("user");
        capturedMessages[1].Content.Should().Contain("Doe John");

        _mockChatProvider.Verify(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo tiểu sử thành công
    /// khi không được cung cấp dữ liệu hệ thống và sử dụng tông giọng trung lập.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và một Member, thêm vào context. Thiết lập _mockUser.Id trả về một giá trị hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanAccessFamily trả về true (hoặc IsAdmin = true).
    ///               Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi tiểu sử hợp lệ.
    ///               Tạo một GenerateBiographyCommand với UseSystemData = false và Tone = Neutral.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value chứa nội dung tiểu sử không rỗng.
    ///              Kiểm tra các thông điệp được gửi đến ChatProvider để đảm bảo chúng không chứa dữ liệu hệ thống
    ///              và yêu cầu tông giọng trung lập.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể tạo tiểu sử
    /// mà không cần dựa vào dữ liệu hệ thống và vẫn tuân thủ yêu cầu về tông giọng trung lập, trả về
    /// kết quả thành công khi quá trình này diễn ra đúng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateBiographySuccessfully_WithoutSystemDataAndNeutralTone()
    {
        var family = _fixture.Create<Family>();
        var member = _fixture.Build<Member>()
            .With(m => m.FamilyId, family.Id)
            .With(m => m.Family, family)
            .With(m => m.FirstName, "Jane")
            .With(m => m.LastName, "Doe")
            .Create();
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(member.FamilyId)).Returns(true); // Assume authorized

        List<ChatMessage>? capturedMessages = null;
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .Callback<List<ChatMessage>>(messages => capturedMessages = messages)
                         .ReturnsAsync("This is a generated biography without system data and a neutral tone.");

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .With(c => c.UseSystemData, false)
            .With(c => c.Tone, BiographyTone.Neutral)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().NotBeEmpty();
        result.Value.Content.Should().Contain("This is a generated biography");

        capturedMessages.Should().NotBeNull();
        if (capturedMessages == null) throw new Xunit.Sdk.XunitException("capturedMessages should not be null.");
        capturedMessages.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("neutral, objective, and informative tone");
        capturedMessages[1].Role.Should().Be("user");
        capturedMessages[1].Content.Should().Contain("Doe Jane");
        capturedMessages[1].Content.Should().NotContain("Here is additional system data");

        _mockChatProvider.Verify(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cắt bớt nội dung tiểu sử
    /// nếu độ dài của nó vượt quá giới hạn từ cho phép.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và một Member, thêm vào context. Thiết lập _mockUser.Id trả về một giá trị hợp lệ.
    ///               Thiết lập _mockAuthorizationService để CanAccessFamily trả về true (hoặc IsAdmin = true).
    ///               Thiết lập _mockChatProvider để GenerateResponseAsync trả về một chuỗi tiểu sử rất dài
    ///               (ví dụ: hơn 1500 từ).
    ///               Tạo một GenerateBiographyCommand cho thành viên đó.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value chứa nội dung tiểu sử đã được cắt bớt.
    ///              Kiểm tra rằng độ dài của tiểu sử đã cắt bớt không vượt quá giới hạn và kết thúc bằng "...".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống xử lý đúng
    /// các tiểu sử dài do AI tạo ra bằng cách cắt bớt chúng để phù hợp với giới hạn lưu trữ hoặc hiển thị,
    /// đồng thời thêm dấu hiệu cắt bớt để người dùng biết nội dung đã bị rút gọn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldTruncateBiography_WhenExceedsWordLimit()
    {
        var family = _fixture.Create<Family>();
        var member = _fixture.Build<Member>()
            .With(m => m.FamilyId, family.Id)
            .With(m => m.Family, family)
            .Create();
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.CanAccessFamily(member.FamilyId)).Returns(true); // Assume authorized

        var longBiography = string.Join(" ", Enumerable.Repeat("word", 2000));
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(longBiography);

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().NotBeEmpty();
        result.Value.Content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length.Should().BeLessThanOrEqualTo(1500);
        result.Value.Content.Should().EndWith("...");

        _mockChatProvider.Verify(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
    }
}
