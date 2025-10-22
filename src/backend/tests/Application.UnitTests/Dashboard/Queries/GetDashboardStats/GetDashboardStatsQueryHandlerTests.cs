using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandlerTests : TestBase
{
    private readonly GetDashboardStatsQueryHandler _handler;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;

    public GetDashboardStatsQueryHandlerTests()
    {
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _handler = new GetDashboardStatsQueryHandler(_context, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockAuthorizationService để trả về null cho GetCurrentUserProfileAsync.
        // 2. Tạo một GetDashboardStatsQuery bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((UserProfile)null!); // UserProfile not found

        var query = _fixture.Create<GetDashboardStatsQuery>();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu hồ sơ người dùng không tồn tại trong hệ thống,
        // yêu cầu lấy thống kê dashboard sẽ thất bại để ngăn chặn việc truy cập dữ liệu không hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStats_WhenUserIsAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về thống kê dashboard cho tất cả các gia đình
        // khi người dùng là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile và IsAdmin là true.
        // 3. Thêm một số gia đình, thành viên và mối quan hệ vào DB.
        // 4. Tạo một GetDashboardStatsQuery bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem DashboardStatsDto có chứa các giá trị thống kê chính xác.

        // Arrange
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

        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()));
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        var query = _fixture.Build<GetDashboardStatsQuery>().Without(q => q.FamilyId).Create();

        _context.Families.Should().HaveCount(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2);
        result.Value.TotalMembers.Should().Be(3);
        result.Value.TotalRelationships.Should().Be(1);
        result.Value.TotalGenerations.Should().Be(0); // Placeholder

        // 💡 Giải thích:
        // Test này đảm bảo rằng một quản trị viên có thể truy xuất thống kê tổng thể
        // cho tất cả các gia đình, thành viên và mối quan hệ trong hệ thống.
    }
}
