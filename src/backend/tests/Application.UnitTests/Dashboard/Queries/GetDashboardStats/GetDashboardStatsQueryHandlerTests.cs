using backend.Application.Common.Constants;
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
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id trả về null để mô phỏng UserProfile không tìm thấy.
    ///    - Act: Gọi phương thức Handle của handler với một GetDashboardStatsQuery bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.UserProfileNotFound
    ///              và ErrorSource là ErrorSources.NotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải kiểm tra sự tồn tại của hồ sơ người dùng
    /// sau khi xác thực để đảm bảo tính toàn vẹn dữ liệu và ngăn chặn các lỗi không mong muốn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns((Guid?)null); // Simulate UserProfile not found

        var query = _fixture.Create<GetDashboardStatsQuery>();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);
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

        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString(), Email = "admin@example.com", Name = "Admin User" };
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
}
