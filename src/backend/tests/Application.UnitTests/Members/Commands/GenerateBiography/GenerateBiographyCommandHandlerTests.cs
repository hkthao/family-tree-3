using backend.Application.Members.Commands.GenerateBiography;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using Moq;
using backend.Domain.Enums;
using backend.Application.AI.Common;
using backend.Application.Common.Interfaces; // Added for IChatProviderFactory and IChatProvider

namespace backend.Application.UnitTests.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandHandlerTests : TestBase
{
    private readonly GenerateBiographyCommandHandler _handler;
    private readonly Mock<IChatProviderFactory> _mockChatProviderFactory; // Added
    private readonly Mock<IChatProvider> _mockChatProvider; // Added

    public GenerateBiographyCommandHandlerTests()
    {
        _mockChatProviderFactory = new Mock<IChatProviderFactory>(); // Added
        _mockChatProvider = new Mock<IChatProvider>(); // Added
        _mockChatProviderFactory.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_mockChatProvider.Object); // Added

        _handler = new GenerateBiographyCommandHandler(_context, _mockUser.Object, _mockAuthorizationService.Object, _mockChatProviderFactory.Object); // Updated
    }

    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.AIBiographies.RemoveRange(_context.AIBiographies);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
            _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
            await _context.SaveChangesAsync(CancellationToken.None);

            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
            _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
        }
        else
        {
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile)null!);
        }
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi chức năng tạo tiểu sử AI không được hỗ trợ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler luôn trả về kết quả thất bại với thông báo
    /// "AI biography generation is currently not supported." do chức năng này đã bị loại bỏ.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_AIGenerationNotSupported()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new GenerateBiographyCommand { MemberId = memberId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("AI did not return a biography."); // Updated expected error message
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng hiện tại không được xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu _user.Id là null hoặc rỗng, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi xác thực.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthenticated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.AIBiographies.RemoveRange(_context.AIBiographies);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns((string)null!);

        var command = new GenerateBiographyCommand { MemberId = Guid.NewGuid() };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi UserProfile của người dùng không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu UserProfile không được tìm thấy cho người dùng hiện tại,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new GenerateBiographyCommand { MemberId = memberId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi thành viên không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cố gắng tạo tiểu sử AI cho một thành viên không tồn tại,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MemberNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var nonExistentMemberId = Guid.NewGuid();
        var command = new GenerateBiographyCommand { MemberId = nonExistentMemberId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền tạo tiểu sử AI.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình hoặc Admin
    /// mới có thể tạo tiểu sử AI. Nếu người dùng không có quyền, handler sẽ trả về kết quả thất bại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, false); // User is not authorized

        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new GenerateBiographyCommand { MemberId = memberId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can generate biographies.");
    }
}