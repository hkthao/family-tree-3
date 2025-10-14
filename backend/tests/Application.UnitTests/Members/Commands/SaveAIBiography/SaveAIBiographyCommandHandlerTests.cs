using backend.Application.Members.Commands.SaveAIBiography;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Members.Commands.SaveAIBiography;

public class SaveAIBiographyCommandHandlerTests : TestBase
{
    private readonly SaveAIBiographyCommandHandler _handler;

    public SaveAIBiographyCommandHandlerTests()
    {
        _handler = new SaveAIBiographyCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object);
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
    /// Kiểm tra xem có thể tạo mới một tiểu sử AI thành công.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh SaveAIBiographyCommand hợp lệ được thực thi
    /// bởi một người dùng có quyền quản lý gia đình, một tiểu sử AI mới sẽ được tạo và lưu vào cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_CreateNewAIBiography_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SaveAIBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            Provider = AIProviderType.OpenAI,
            UserPrompt = "Viết tiểu sử trang trọng.",
            GeneratedFromDB = false,
            TokensUsed = 100
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var createdBiography = await _context.AIBiographies.FirstOrDefaultAsync(b => b.MemberId == memberId && b.Style == BiographyStyle.Formal);
        createdBiography.Should().NotBeNull();
        createdBiography!.Content.Should().Be("Đây là nội dung tiểu sử AI mới.");
    }

    /// <summary>
    /// Kiểm tra xem có thể cập nhật một tiểu sử AI hiện có thành công.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh SaveAIBiographyCommand được thực thi để cập nhật một tiểu sử AI hiện có,
    /// nội dung của tiểu sử đó sẽ được thay đổi trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_UpdateExistingAIBiography_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true);

        var memberId = Guid.NewGuid();
        _context.Members.Add(new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = familyId });
        var existingBiography = new AIBiography
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            Provider = AIProviderType.OpenAI,
            UserPrompt = "Prompt cũ.",
            GeneratedFromDB = false,
            TokensUsed = 50,
            Content = "Nội dung cũ."
        };
        _context.AIBiographies.Add(existingBiography);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SaveAIBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            Content = "Nội dung cập nhật.",
            Provider = AIProviderType.Gemini,
            UserPrompt = "Prompt cập nhật.",
            GeneratedFromDB = true,
            TokensUsed = 120
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        var updatedBiography = await _context.AIBiographies.FirstOrDefaultAsync(b => b.MemberId == memberId && b.Style == BiographyStyle.Formal);
        updatedBiography.Should().NotBeNull();
        updatedBiography!.Content.Should().Be("Nội dung cập nhật.");
        updatedBiography.Provider.Should().Be(AIProviderType.Gemini);
        updatedBiography.TokensUsed.Should().Be(120);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi thành viên không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cố gắng lưu tiểu sử AI cho một thành viên không tồn tại,
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
        var command = new SaveAIBiographyCommand
        {
            Provider = AIProviderType.OpenAI,
            UserPrompt = "Prompt.",
            GeneratedFromDB = false,
            TokensUsed = 10
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");
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

        var command = new SaveAIBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            Content = "Nội dung.",
            Provider = AIProviderType.OpenAI,
            UserPrompt = "Prompt.",
            GeneratedFromDB = false,
            TokensUsed = 10
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền lưu tiểu sử AI.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền quản lý gia đình hoặc Admin
    /// mới có thể lưu tiểu sử AI. Nếu người dùng không có quyền, handler sẽ trả về kết quả thất bại.
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

        var command = new SaveAIBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            Content = "Nội dung.",
            Provider = AIProviderType.OpenAI,
            UserPrompt = "Prompt.",
            GeneratedFromDB = false,
            TokensUsed = 10
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can save AI biographies.");
    }
}