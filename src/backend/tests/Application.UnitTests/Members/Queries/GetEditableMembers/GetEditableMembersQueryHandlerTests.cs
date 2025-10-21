using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetEditableMembers;
using backend.Application.Members.Queries.GetMembers; // For MemberListDto
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.UnitTests.Members.Queries.GetEditableMembers;

public class GetEditableMembersQueryHandlerTests : TestBase
{
    private readonly GetEditableMembersQueryHandler _handler;

    public GetEditableMembersQueryHandlerTests()
    {
        _handler = new GetEditableMembersQueryHandler(_context, _mockUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng chưa được xác thực.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về null.
        // 2. Act: Gọi phương thức Handle với một GetEditableMembersQuery bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockUser.Setup(u => u.Id).Returns((string)null!);

        var query = _fixture.Create<GetEditableMembersQuery>();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not authenticated.");
        // 💡 Giải thích: Handler phải kiểm tra xác thực người dùng trước khi thực hiện các thao tác khác.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserManagesNoFamilies()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi người dùng không quản lý gia đình nào.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock _mockUser.Id trả về một ID hợp lệ. Đảm bảo không có FamilyUser nào cho người dùng này.
        // 2. Act: Gọi phương thức Handle với một GetEditableMembersQuery bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách thành viên rỗng.
        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // No FamilyUser entries for this userId in _context

        var query = _fixture.Create<GetEditableMembersQuery>();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Nếu người dùng không quản lý gia đình nào, không có thành viên nào có thể chỉnh sửa.
    }

    [Fact]
    public async Task Handle_ShouldReturnMembers_WhenUserManagesFamilies()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách thành viên có thể chỉnh sửa khi người dùng quản lý gia đình. 
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập dữ liệu thủ công cho Family, Member, UserProfile, FamilyUser.
        //             Đảm bảo người dùng là quản lý/admin của một số gia đình.
        // 2. Act: Gọi phương thức Handle với một GetEditableMembersQuery bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa các MemberListDto mong đợi.
        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // Family 1 (Managed by user)
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family1);
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "One", Code = "M001" };
        _context.Members.Add(member1);
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "Two", Code = "M002" };
        _context.Members.Add(member2);

        // Family 2 (Managed by user)
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        _context.Families.Add(family2);
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member", LastName = "Three", Code = "M003" };
        _context.Members.Add(member3);

        // Family 3 (Not managed by user)
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family C", Code = "FC001" };
        _context.Families.Add(family3);
        var member4 = new Member { Id = Guid.NewGuid(), FamilyId = family3.Id, FirstName = "Member", LastName = "Four", Code = "M004" };
        _context.Members.Add(member4);

        var userProfile = new UserProfile { Id = Guid.Parse(userId), ExternalId = userId, Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);

        // User manages Family 1 (Manager role)
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Manager });
        // User manages Family 2 (Admin role)
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Admin });
        // User is just a member of Family 3 (should not be editable)
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family3.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });

        await _context.SaveChangesAsync();

        var query = _fixture.Create<GetEditableMembersQuery>();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3); // member1, member2, member3
        result.Value.Should().Contain(m => m.Id == member1.Id && m.FamilyId == family1.Id && m.FamilyName == family1.Name);
        result.Value.Should().Contain(m => m.Id == member2.Id && m.FamilyId == family1.Id && m.FamilyName == family1.Name);
        result.Value.Should().Contain(m => m.Id == member3.Id && m.FamilyId == family2.Id && m.FamilyName == family2.Name);
        result.Value.Should().NotContain(m => m.Id == member4.Id);
        // 💡 Giải thích: Handler chỉ trả về các thành viên thuộc gia đình mà người dùng có quyền quản lý hoặc là admin.
    }

    [Fact]
    public async Task Handle_ShouldNotReturnMembers_WhenUserIsNotManagerOrAdmin()
    {
        // 🎯 Mục tiêu của test: Xác minh handler không trả về thành viên khi người dùng không phải là quản lý hoặc admin.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập dữ liệu thủ công cho Family, Member, UserProfile, FamilyUser.
        //             Đảm bảo người dùng chỉ là thành viên (không phải quản lý/admin) của gia đình.
        // 2. Act: Gọi phương thức Handle với một GetEditableMembersQuery bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách thành viên rỗng.
        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var family = new Family { Id = Guid.NewGuid(), Name = "Family D", Code = "FD001" };
        _context.Families.Add(family);
        var member = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Member", LastName = "Five", Code = "M005" };
        _context.Members.Add(member);

        var userProfile = new UserProfile { Id = Guid.Parse(userId), ExternalId = userId, Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);

        // User is only a member of Family D
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });

        await _context.SaveChangesAsync();

        var query = _fixture.Create<GetEditableMembersQuery>();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Người dùng chỉ là thành viên không có quyền chỉnh sửa thành viên khác trong gia đình.
    }
}
