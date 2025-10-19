using backend.Application.Members.Queries.GetMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests : TestBase
{
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _handler = new GetMembersQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            _context.Families.Add(new Family { Id = familyId, Name = "Test Family", Code = "TESTFAM" });
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
    /// Kiểm tra xem handler có trả về tất cả các thành viên khi người dùng là Admin và không có FamilyId cụ thể.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng một người dùng có quyền Admin có thể truy xuất tất cả các thành viên
    /// có trong cơ sở dữ liệu, bất kể gia đình nào.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnAllMembers_WhenUserIsAdminAndNoFamilyId()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId1, true, true);

        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2" });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = familyId1 });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = familyId2 });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetMembersQuery();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new { FullName = "1 Member" });
        result.Value.Should().ContainEquivalentOf(new { FullName = "2 Member" });
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các thành viên cho một FamilyId cụ thể khi người dùng là Admin.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng Admin có thể lọc thành viên theo FamilyId.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnMembersForSpecificFamily_WhenUserIsAdmin()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId1, true, true);

        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2" });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = familyId1 });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = familyId2 });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetMembersQuery { FamilyId = familyId1 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.FullName.Should().Be("1 Member");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các thành viên từ các gia đình mà người dùng có quyền truy cập
    /// khi người dùng không phải Admin và không có FamilyId cụ thể.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng người dùng không phải Admin chỉ thấy thành viên từ các gia đình họ có quyền.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnMembersFromAccessibleFamilies_WhenUserIsNotAdminAndNoFamilyId()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var accessibleFamilyId = Guid.NewGuid();
        var inaccessibleFamilyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, accessibleFamilyId, false, true);

        _context.Families.Add(new Family { Id = inaccessibleFamilyId, Name = "Inaccessible Family" });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Accessible", LastName = "Member", FamilyId = accessibleFamilyId });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Inaccessible", LastName = "Member", FamilyId = inaccessibleFamilyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetMembersQuery();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.FullName.Should().Contain("Accessible");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các thành viên cho một FamilyId cụ thể
    /// khi người dùng không phải Admin và có quyền truy cập vào FamilyId đó.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng người dùng không phải Admin có thể lọc thành viên theo FamilyId
    /// nếu họ có quyền truy cập vào gia đình đó.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnMembersForSpecificAccessibleFamily_WhenUserIsNotAdmin()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var accessibleFamilyId = Guid.NewGuid();
        var inaccessibleFamilyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, accessibleFamilyId, false, true);

        _context.Families.Add(new Family { Id = inaccessibleFamilyId, Name = "Inaccessible Family" });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Accessible", LastName = "Member", FamilyId = accessibleFamilyId });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Inaccessible", LastName = "Member", FamilyId = inaccessibleFamilyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetMembersQuery { FamilyId = accessibleFamilyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.FullName.Should().Contain("Accessible");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không được xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu _user.Id là null hoặc rỗng, handler sẽ trả về kết quả thất bại
    /// với thông báo lỗi xác thực.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(x => x.Id).Returns((string)null!);

        var query = new GetMembersQuery();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User is not authenticated.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi UserProfile của người dùng không tồn tại (non-Admin).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu UserProfile không được tìm thấy cho người dùng không phải Admin,
    /// handler sẽ trả về một danh sách rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenUserProfileNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, false, true, false); // userProfileExists = false

        var query = new GetMembersQuery();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền truy cập vào FamilyId được yêu cầu (non-Admin).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng nếu người dùng không phải Admin yêu cầu thành viên từ một gia đình
    /// mà họ không có quyền truy cập, handler sẽ trả về kết quả thất bại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNotAuthorizedToViewFamily()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var accessibleFamilyId = Guid.NewGuid();
        var unauthorizedFamilyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, accessibleFamilyId, false, true);

        _context.Families.Add(new Family { Id = unauthorizedFamilyId, Name = "Unauthorized Family" });
        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Unauthorized", LastName = "Member", FamilyId = unauthorizedFamilyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetMembersQuery { FamilyId = unauthorizedFamilyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied to the requested family.");
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi không có thành viên nào tồn tại cho FamilyId được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một FamilyId được cung cấp không có thành viên nào liên quan,
    /// handler sẽ trả về một danh sách thành viên rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoMembersForFamilyId()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var userId = Guid.NewGuid().ToString();
        var userProfileId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupUser(userId, userProfileId, familyId, true, true);

        // Không thêm thành viên nào cho gia đình này.

        var query = new GetMembersQuery { FamilyId = familyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
