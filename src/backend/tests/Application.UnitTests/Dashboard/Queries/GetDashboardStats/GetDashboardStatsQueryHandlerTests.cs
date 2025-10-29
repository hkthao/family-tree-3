using AutoFixture;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// Bộ test cho GetDashboardStatsQueryHandler.
/// </summary>
public class GetDashboardStatsQueryHandlerTests : TestBase
{
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _handler = new GetDashboardStatsQueryHandler(_context, _mockAuthorizationService.Object, _mockUser.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thống kê dashboard cho tất cả các gia đình
    /// khi người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo UserProfile cho admin, thêm các Family, Member, Relationship vào DB.
    ///               Thiết lập _mockAuthorizationService.IsAdmin để trả về true. Thiết lập _mockUser.Id và _mockUser.ExternalId.
    ///    - Act: Gọi phương thức Handle của handler với một GetDashboardStatsQuery không có FamilyId.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. DashboardStatsDto chứa các giá trị thống kê chính xác
    ///              (TotalFamilies, TotalMembers, TotalRelationships). TotalGenerations được đặt là 0 (placeholder).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một quản trị viên phải có quyền truy xuất thống kê tổng thể
    /// cho toàn bộ hệ thống.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllStats_WhenUserIsAdmin()
    {
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "admin@example.com", Name = "Admin User", FirstName = "Admin", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        _context.Families.AddRange(family1, family2);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member1", LastName = "Test", Code = "M1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member2", LastName = "Test", Code = "M2" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member3", LastName = "Test", Code = "M3" };
        _context.Members.AddRange(member1, member2, member3);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Father, FamilyId = family1.Id };
        _context.Relationships.Add(relationship1);

        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockUser.Setup(x => x.ExternalId).Returns(userProfile.ExternalId);
        var query = _fixture.Build<GetDashboardStatsQuery>().Without(q => q.FamilyId).Create();

        _context.Families.Should().HaveCount(2);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2);
        result.Value.TotalMembers.Should().Be(3);
        result.Value.TotalRelationships.Should().Be(1);
        result.Value.TotalGenerations.Should().Be(0); 
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thống kê dashboard cho một gia đình cụ thể
    /// khi FamilyId được cung cấp và người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo UserProfile cho admin, thêm các Family, Member, Relationship vào DB.
    ///               Thiết lập _mockAuthorizationService.IsAdmin để trả về true. Thiết lập _mockUser.Id và _mockUser.ExternalId.
    ///               Tạo một GetDashboardStatsQuery với FamilyId của một gia đình cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. DashboardStatsDto chứa các giá trị thống kê chính xác
    ///              chỉ cho gia đình được chỉ định.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Quản trị viên có thể xem thống kê chi tiết cho từng gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnStatsForSpecificFamily_WhenFamilyIdIsProvidedAndUserIsAdmin()
    {
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "admin@example.com", Name = "Admin User", FirstName = "Admin", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        _context.Families.AddRange(family1, family2);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member1", LastName = "Test", Code = "M1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member2", LastName = "Test", Code = "M2" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member3", LastName = "Test", Code = "M3" };
        _context.Members.AddRange(member1, member2, member3);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Father, FamilyId = family1.Id };
        _context.Relationships.Add(relationship1);

        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockUser.Setup(x => x.ExternalId).Returns(userProfile.ExternalId);
        var query = _fixture.Build<GetDashboardStatsQuery>().With(q => q.FamilyId, family1.Id).Create();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1);
        result.Value.TotalMembers.Should().Be(2); // member1, member2 belong to family1
        result.Value.TotalRelationships.Should().Be(1); // relationship1 belongs to family1
        result.Value.TotalGenerations.Should().Be(0);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thống kê dashboard cho các gia đình mà người dùng có quyền truy cập
    /// khi người dùng không phải là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo UserProfile cho người dùng, thêm các Family, Member, Relationship vào DB.
    ///               Thiết lập _mockAuthorizationService.IsAdmin để trả về false.
    ///               Thiết lập _mockUser.Id và _mockUser.ExternalId.
    ///               Thêm FamilyUser để người dùng có quyền truy cập vào một số gia đình.
    ///    - Act: Gọi phương thức Handle của handler với một GetDashboardStatsQuery không có FamilyId.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. DashboardStatsDto chứa các giá trị thống kê chính xác
    ///              chỉ cho các gia đình mà người dùng có quyền truy cập.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng không phải quản trị viên chỉ có thể xem thống kê
    /// cho các gia đình mà họ được cấp quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnStatsForAccessibleFamilies_WhenUserIsNotAdmin()
    {
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "user@example.com", Name = "Normal User", FirstName = "Normal", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3" };
        _context.Families.AddRange(family1, family2, family3);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member1", LastName = "Test", Code = "M1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member2", LastName = "Test", Code = "M2" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member3", LastName = "Test", Code = "M3" };
        var member4 = new Member { Id = Guid.NewGuid(), FamilyId = family3.Id, FirstName = "Member4", LastName = "Test", Code = "M4" };
        _context.Members.AddRange(member1, member2, member3, member4);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Father, FamilyId = family1.Id };
        _context.Relationships.Add(relationship1);

        var familyUser1 = new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id };
        var familyUser2 = new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfile.Id };
        _context.FamilyUsers.AddRange(familyUser1, familyUser2);

        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockUser.Setup(x => x.ExternalId).Returns(userProfile.ExternalId);
        var query = _fixture.Build<GetDashboardStatsQuery>().Without(q => q.FamilyId).Create();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2); // family1, family2
        result.Value.TotalMembers.Should().Be(3); // member1, member2 (family1), member3 (family2)
        result.Value.TotalGenerations.Should().Be(0);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về thống kê dashboard cho một gia đình cụ thể mà người dùng có quyền truy cập
    /// khi FamilyId được cung cấp và người dùng không phải là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo UserProfile cho người dùng, thêm các Family, Member, Relationship vào DB.
    ///               Thiết lập _mockAuthorizationService.IsAdmin để trả về false.
    ///               Thiết lập _mockUser.Id và _mockUser.ExternalId.
    ///               Thêm FamilyUser để người dùng có quyền truy cập vào một số gia đình.
    ///               Tạo một GetDashboardStatsQuery với FamilyId của một gia đình cụ thể mà người dùng có quyền truy cập.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. DashboardStatsDto chứa các giá trị thống kê chính xác
    ///              chỉ cho gia đình được chỉ định.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng không phải quản trị viên chỉ có thể xem thống kê
    /// cho các gia đình mà họ được cấp quyền, và nếu FamilyId được cung cấp, nó phải là một trong số đó.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnStatsForSpecificAccessibleFamily_WhenFamilyIdIsProvidedAndUserIsNotAdmin()
    {
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "user@example.com", Name = "Normal User", FirstName = "Normal", LastName = "User", Phone = "1234567890" };
        _context.UserProfiles.Add(userProfile);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        _context.Families.AddRange(family1, family2);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member1", LastName = "Test", Code = "M1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member2", LastName = "Test", Code = "M2" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member3", LastName = "Test", Code = "M3" };
        _context.Members.AddRange(member1, member2, member3);

        var relationship1 = new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Father, FamilyId = family1.Id };
        _context.Relationships.Add(relationship1);

        var familyUser1 = new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfile.Id };
        var familyUser2 = new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfile.Id };
        _context.FamilyUsers.AddRange(familyUser1, familyUser2);

        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockUser.Setup(x => x.Id).Returns(userProfile.Id);
        _mockUser.Setup(x => x.ExternalId).Returns(userProfile.ExternalId);
        var query = _fixture.Build<GetDashboardStatsQuery>().With(q => q.FamilyId, family1.Id).Create();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1); // family1
        result.Value.TotalMembers.Should().Be(2); // member1, member2 (family1)
        result.Value.TotalRelationships.Should().Be(1); // relationship1 (family1)
        result.Value.TotalGenerations.Should().Be(0);
    }
}
