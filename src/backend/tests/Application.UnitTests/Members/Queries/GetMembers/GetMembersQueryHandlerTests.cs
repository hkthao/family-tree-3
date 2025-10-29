using backend.Application.Common.Constants;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests : TestBase
{
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _handler = new GetMembersQueryHandler(
            _context,
            _mapper,
            _mockUser.Object,
            _mockAuthorizationService.Object
        );
    }


    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả thành viên khi người dùng là quản trị viên và không có FamilyId cụ thể.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true.
    ///               Thêm nhiều gia đình và thành viên vào Context.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery không có FamilyId.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chứa tất cả thành viên.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Quản trị viên có quyền xem tất cả thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllMembers_WhenAdminAndNoFamilyId()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family1);
        _context.Members.AddRange(member1, member2);

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Peter", LastName = "Pan", Code = "M003" };
        _context.Families.Add(family2);
        _context.Members.Add(member3);
        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = null }; // No specific FamilyId

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3); // All members
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
        result.Value.Should().Contain(m => m.Id == member3.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thành viên của gia đình cụ thể khi người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true.
    ///               Thêm nhiều gia đình và thành viên vào Context.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên của gia đình đó.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Quản trị viên có thể lọc thành viên theo FamilyId.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFamilyMembers_WhenAdminAndFamilyIdProvided()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family1);
        _context.Members.AddRange(member1, member2);

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Peter", LastName = "Pan", Code = "M003" };
        _context.Families.Add(family2);
        _context.Members.Add(member3);
        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = family1.Id }; // Specific FamilyId

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // Only members from Family A
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
        result.Value.Should().NotContain(m => m.Id == member3.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về danh sách rỗng khi người dùng không phải quản trị viên và không có UserProfile.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
    ///               Đảm bảo không có UserProfile nào trong Context khớp với _mockUser.Id.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery bất kỳ.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và danh sách rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng không có UserProfile thì không có quyền truy cập gia đình nào.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNonAdminAndNoUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        // Ensure no UserProfile exists for the mocked user ID
        _context.UserProfiles.RemoveRange(_context.UserProfiles.Where(up => up.Id == userId));
        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = null, SearchTerm = null };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thành viên từ các gia đình mà người dùng có quyền truy cập
    /// (không phải quản trị viên, không có FamilyId cụ thể).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
    ///               Thiết lập UserProfile với FamilyUsers cho các gia đình cụ thể.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery không có FamilyId.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên từ các gia đình có quyền truy cập.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng chỉ có thể xem thành viên từ các gia đình mà họ có quyền truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessibleFamilyMembers_WhenNonAdminAndNoFamilyId()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User", FirstName = "Test", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        _context.Families.Add(family1);
        _context.Members.Add(member1);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer }); // Accessible

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family2);
        _context.Members.Add(member2);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer }); // Accessible

        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = null };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // member1 and member2
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thành viên của gia đình cụ thể mà người dùng có quyền truy cập (không phải quản trị viên).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
    ///               Thiết lập UserProfile với FamilyUsers cho các gia đình.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể và có quyền truy cập.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên của gia đình đó.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng có thể lọc thành viên theo FamilyId nếu họ có quyền truy cập gia đình đó.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSpecificFamilyMembers_WhenNonAdminAndAccessibleFamilyId()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User", FirstName = "Test", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        _context.Families.Add(family1);
        _context.Members.Add(member1);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer }); // Accessible

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family2);
        _context.Members.Add(member2);
        // No FamilyUser for family2, so not accessible

        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = family1.Id }; // Specific accessible FamilyId

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1); // Only member1
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().NotContain(m => m.Id == member2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi người dùng không phải quản trị viên và yêu cầu FamilyId không có quyền truy cập.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
    ///               Thiết lập UserProfile với FamilyUsers cho các gia đình.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể và không có quyền truy cập.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng không thể xem thành viên từ các gia đình mà họ không có quyền truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNonAdminAndInaccessibleFamilyId()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User", FirstName = "Test", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        _context.Families.Add(family1);
        _context.Members.Add(member1);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer }); // Accessible

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family2);
        _context.Members.Add(member2);
        // No FamilyUser for family2, so not accessible

        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = family2.Id }; // Specific inaccessible FamilyId

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc thành viên theo SearchTerm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true (để đơn giản hóa quyền).
    ///               Thêm nhiều thành viên với tên khác nhau vào Context.
    ///    - Act: Gọi phương thức Handle với GetMembersQuery có SearchTerm.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên khớp với SearchTerm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải lọc thành viên theo SearchTerm.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true); // Admin to bypass family access checks

        var family = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Smith", Code = "M002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Peter", LastName = "Jones", Code = "M003" };
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { SearchTerm = "john" }; // Search for "john"

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1); // Only member1
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().NotContain(m => m.Id == member2.Id);
        result.Value.Should().NotContain(m => m.Id == member3.Id);
    }
}
