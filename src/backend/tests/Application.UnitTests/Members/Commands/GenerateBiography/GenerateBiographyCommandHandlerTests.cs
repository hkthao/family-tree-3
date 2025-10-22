using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
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
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory;
    private readonly Mock<FamilyAuthorizationService> _mockFamilyAuthorizationService;
    private readonly Mock<IChatProvider> _mockChatProvider;
    private readonly GenerateBiographyCommandHandler _handler;

    public GenerateBiographyCommandHandlerTests()
    {
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockChatProviderFactory = _fixture.Freeze<Mock<IChatProviderFactory>>();
        _mockFamilyAuthorizationService = new Mock<FamilyAuthorizationService>(_context, _mockUser.Object, _mockAuthorizationService.Object);
        _mockChatProvider = new Mock<IChatProvider>();

        _mockChatProviderFactory.Setup(f => f.GetProvider(It.IsAny<ChatAIProvider>()))
                                .Returns(_mockChatProvider.Object);

        _handler = new GenerateBiographyCommandHandler(
            _context,
            _mockUser.Object,
            _mockAuthorizationService.Object,
            _mockChatProviderFactory.Object,
            _mockFamilyAuthorizationService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng chưa được xác thực.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id trả về null hoặc chuỗi rỗng.
        // 2. Act: Gọi phương thức Handle với một GenerateBiographyCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockUser.Setup(u => u.Id).Returns((string)null!); // User not authenticated

        var command = _fixture.Create<GenerateBiographyCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");
        result.ErrorSource.Should().Be("Authentication");
        // 💡 Giải thích: Handler phải kiểm tra xác thực người dùng trước khi thực hiện các thao tác khác.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy UserProfile.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id trả về một giá trị hợp lệ. Mock GetCurrentUserProfileAsync trả về null.
        // 2. Act: Gọi phương thức Handle với một GenerateBiographyCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString()); // User authenticated
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile)null!); // Profile not found

        var command = _fixture.Create<GenerateBiographyCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Handler phải kiểm tra UserProfile trước khi thực hiện các thao tác khác.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id trả về một giá trị hợp lệ. Mock GetCurrentUserProfileAsync trả về profile hợp lệ.
        //             Đảm bảo _context.Members không chứa thành viên cần tìm.
        // 2. Act: Gọi phương thức Handle với một GenerateBiographyCommand có MemberId không tồn tại.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_fixture.Create<UserProfile>());

        var nonExistentMemberId = Guid.NewGuid();
        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, nonExistentMemberId)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Handler phải kiểm tra sự tồn tại của thành viên trước khi tạo tiểu sử.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthorized()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id trả về một giá trị hợp lệ. Mock GetCurrentUserProfileAsync trả về profile hợp lệ.
        //             Thêm một thành viên vào DB. Mock AuthorizeFamilyAccess trả về kết quả thất bại.
        // 2. Act: Gọi phương thức Handle với một GenerateBiographyCommand cho thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userProfile = _fixture.Create<UserProfile>();
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockFamilyAuthorizationService.Setup(f => f.AuthorizeFamilyAccess(member.FamilyId, It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result<Family>.Failure("Access denied.", "Authorization"));

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied.");
        result.ErrorSource.Should().Be("Authorization");
        // 💡 Giải thích: Người dùng phải có quyền truy cập vào gia đình của thành viên để tạo tiểu sử.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAIGeneratesEmptyBiography()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi AI tạo ra tiểu sử trống.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id và GetCurrentUserProfileAsync trả về giá trị hợp lệ.
        //             Thêm một thành viên vào DB. Mock AuthorizeFamilyAccess trả về thành công.
        //             Mock IChatProvider.GenerateResponseAsync trả về chuỗi rỗng hoặc khoảng trắng.
        // 2. Act: Gọi phương thức Handle với một GenerateBiographyCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userProfile = _fixture.Create<UserProfile>();
        var member = _fixture.Create<Member>();
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockFamilyAuthorizationService.Setup(f => f.AuthorizeFamilyAccess(member.FamilyId, It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result<Family>.Success(member.Family));
        _mockChatProvider.Setup(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()))
                         .ReturnsAsync(string.Empty); // AI generates empty biography

        var command = _fixture.Build<GenerateBiographyCommand>()
            .With(c => c.MemberId, member.Id)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI did not return a biography.");
        result.ErrorSource.Should().Be("NoContent");
        // 💡 Giải thích: Handler phải xử lý trường hợp AI không tạo ra tiểu sử.
    }

    [Fact]
    public async Task Handle_ShouldGenerateBiographySuccessfully_WithSystemDataAndSpecificTone()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo tiểu sử thành công với dữ liệu hệ thống và tông giọng cụ thể.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id và GetCurrentUserProfileAsync trả về giá trị hợp lệ.
        //             Thêm một thành viên và gia đình vào DB. Mock AuthorizeFamilyAccess trả về thành công.
        //             Mock IChatProvider.GenerateResponseAsync trả về một tiểu sử hợp lệ.
        // 2. Act: Gọi phương thức Handle với GenerateBiographyCommand có UseSystemData = true và Tone cụ thể.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và tiểu sử được tạo ra không rỗng.
        var userProfile = _fixture.Create<UserProfile>();
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

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockFamilyAuthorizationService.Setup(f => f.AuthorizeFamilyAccess(member.FamilyId, It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result<Family>.Success(member.Family));
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

        // Assert captured messages
        capturedMessages.Should().NotBeNull();
        if (capturedMessages == null) throw new Xunit.Sdk.XunitException("capturedMessages should not be null.");
        capturedMessages.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("historical and factual tone");
        capturedMessages[1].Role.Should().Be("user");
        capturedMessages[1].Content.Should().Contain("Doe John");

        _mockChatProvider.Verify(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
        // 💡 Giải thích: Handler phải tạo tiểu sử thành công khi có dữ liệu hệ thống và tông giọng cụ thể.
    }

    [Fact]
    public async Task Handle_ShouldGenerateBiographySuccessfully_WithoutSystemDataAndNeutralTone()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo tiểu sử thành công mà không có dữ liệu hệ thống và tông giọng trung lập.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id và GetCurrentUserProfileAsync trả về giá trị hợp lệ.
        //             Thêm một thành viên và gia đình vào DB. Mock AuthorizeFamilyAccess trả về thành công.
        //             Mock IChatProvider.GenerateResponseAsync trả về một tiểu sử hợp lệ.
        // 2. Act: Gọi phương thức Handle với GenerateBiographyCommand có UseSystemData = false và Tone = Neutral.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và tiểu sử được tạo ra không rỗng.
        var userProfile = _fixture.Create<UserProfile>();
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

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockFamilyAuthorizationService.Setup(f => f.AuthorizeFamilyAccess(member.FamilyId, It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result<Family>.Success(member.Family));
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

        // Assert captured messages
        capturedMessages.Should().NotBeNull();
        if (capturedMessages == null) throw new Xunit.Sdk.XunitException("capturedMessages should not be null.");
        capturedMessages.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("neutral, objective, and informative tone");
        capturedMessages[1].Role.Should().Be("user");
        capturedMessages[1].Content.Should().Contain("Doe Jane");
        capturedMessages[1].Content.Should().NotContain("Here is additional system data"); // Should not include system data

        _mockChatProvider.Verify(c => c.GenerateResponseAsync(It.IsAny<List<ChatMessage>>()), Times.Once);
        // 💡 Giải thích: Handler phải tạo tiểu sử thành công mà không có dữ liệu hệ thống và tông giọng trung lập.
    }

    [Fact]
    public async Task Handle_ShouldTruncateBiography_WhenExceedsWordLimit()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cắt bớt tiểu sử nếu nó vượt quá giới hạn từ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _user.Id và GetCurrentUserProfileAsync trả về giá trị hợp lệ.
        //             Thêm một thành viên và gia đình vào DB. Mock AuthorizeFamilyAccess trả về thành công.
        //             Mock IChatProvider.GenerateResponseAsync trả về một tiểu sử rất dài (hơn 1500 từ).
        // 2. Act: Gọi phương thức Handle với GenerateBiographyCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và tiểu sử được cắt bớt.
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var member = _fixture.Build<Member>()
            .With(m => m.FamilyId, family.Id)
            .With(m => m.Family, family)
            .Create();
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockFamilyAuthorizationService.Setup(f => f.AuthorizeFamilyAccess(member.FamilyId, It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result<Family>.Success(member.Family));

        // Create a very long biography (e.g., 2000 words)
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
        // 💡 Giải thích: Handler phải cắt bớt tiểu sử nếu nó vượt quá giới hạn từ.
    }
}
