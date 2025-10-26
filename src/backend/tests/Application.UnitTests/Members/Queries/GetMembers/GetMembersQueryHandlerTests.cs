using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
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
        _handler = new GetMembersQueryHandler(
            _context,
            _mapper,
            _mockUser.Object,
            _mockAuthorizationService.Object
        );
    }


    [Fact]
    public async Task Handle_ShouldReturnAllMembers_WhenAdminAndNoFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về tất cả thành viên khi người dùng là admin và không có FamilyId cụ thể.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true. 
        //             Thêm nhiều gia đình và thành viên vào Context.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery không có FamilyId.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa tất cả thành viên.
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
        // 💡 Giải thích: Admin có quyền xem tất cả thành viên.
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyMembers_WhenAdminAndFamilyIdProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành viên của gia đình cụ thể khi người dùng là admin.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true.
        //             Thêm nhiều gia đình và thành viên vào Context.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên của gia đình đó.
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
        // 💡 Giải thích: Admin có thể lọc thành viên theo FamilyId.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNonAdminAndNoUserProfile()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi người dùng không phải admin và không có UserProfile.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
        //             Mock _mockAuthorizationService.GetCurrentUserProfileAsync() trả về null.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách rỗng.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var query = _fixture.Create<GetMembersQuery>();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Người dùng không có UserProfile thì không có quyền truy cập gia đình nào.
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessibleFamilyMembers_WhenNonAdminAndNoFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành viên từ các gia đình mà người dùng có quyền truy cập (không phải admin, không có FamilyId cụ thể).
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
        //             Thiết lập UserProfile với FamilyUsers cho các gia đình cụ thể.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery không có FamilyId.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên từ các gia đình có quyền truy cập.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User" };
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
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Manager }); // Accessible

        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family C", Code = "FC001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family3.Id, FirstName = "Peter", LastName = "Pan", Code = "M003" };
        _context.Families.Add(family3);
        _context.Members.Add(member3);
        // No FamilyUser for family3, so not accessible

        await _context.SaveChangesAsync();

        var query = new GetMembersQuery { FamilyId = null };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // member1 and member2
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
        result.Value.Should().NotContain(m => m.Id == member3.Id);
        // 💡 Giải thích: Người dùng chỉ có thể xem thành viên từ các gia đình mà họ có quyền truy cập.
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecificFamilyMembers_WhenNonAdminAndAccessibleFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về thành viên của gia đình cụ thể mà người dùng có quyền truy cập (không phải admin).
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
        //             Thiết lập UserProfile với FamilyUsers cho các gia đình.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể và có quyền truy cập.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên của gia đình đó.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User" };
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
        // 💡 Giải thích: Người dùng có thể lọc thành viên theo FamilyId nếu họ có quyền truy cập gia đình đó.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNonAdminAndInaccessibleFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không phải admin và yêu cầu FamilyId không có quyền truy cập.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về false.
        //             Thiết lập UserProfile với FamilyUsers cho các gia đình.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery có FamilyId cụ thể và không có quyền truy cập.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);

        var userProfile = new UserProfile { Id = userId, ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User" };
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
        result.Error.Should().Contain("Access denied to the requested family.");
        // 💡 Giải thích: Người dùng không thể xem thành viên từ các gia đình mà họ không có quyền truy cập.
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc thành viên theo SearchTerm.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về ID hợp lệ. Mock _mockAuthorizationService.IsAdmin() trả về true (để đơn giản hóa quyền).
        //             Thêm nhiều thành viên với tên khác nhau vào Context.
        // 2. Act: Gọi phương thức Handle với GetMembersQuery có SearchTerm.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên khớp với SearchTerm.
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
        // 💡 Giải thích: Handler phải lọc thành viên theo SearchTerm.
    }
}
