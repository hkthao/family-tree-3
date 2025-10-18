
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : TestBase
{
    private readonly GetFamiliesQueryHandler _handler;

    public GetFamiliesQueryHandlerTests()
    {
        _handler = new GetFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về tất cả các gia đình khi người dùng là Admin.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng một người dùng có quyền Admin có thể truy xuất tất cả các gia đình
    /// có trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_All_Families_WhenUserIsAdmin()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình và thành viên hiện có để đảm bảo môi trường test sạch.
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Giả lập UserProfile với Id là Guid và quyền Admin.
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Thêm các gia đình vào cơ sở dữ liệu.
        var families = new List<Family>
        {
            new Family { Id = Guid.NewGuid(), Name = "Gia đình 1", Code = "FAM001" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình 2", Code = "FAM002" }
        };
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về đúng số lượng gia đình.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new { Id = families[0].Id, Name = "Gia đình 1" });
        result.Value.Should().ContainEquivalentOf(new { Id = families[1].Id, Name = "Gia đình 2" });
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách rỗng khi không có gia đình nào tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi không có gia đình nào trong cơ sở dữ liệu,
    /// handler sẽ trả về một danh sách rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoFamiliesExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình và thành viên hiện có để đảm bảo môi trường test sạch.
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Giả lập UserProfile với Id là Guid và quyền Admin.
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về danh sách rỗng.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các gia đình mà người dùng quản lý khi người dùng không phải Admin.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng một người dùng không có quyền Admin chỉ có thể truy xuất các gia đình
    /// mà họ có quyền quản lý.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnManagedFamilies_WhenUserIsNotAdmin()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình và thành viên hiện có để đảm bảo môi trường test sạch.
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid().ToString();
        _mockUser.Setup(x => x.Id).Returns(userId);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        // Tạo một UserProfile cho người dùng hiện tại.
        var currentUserProfile = new UserProfile { Id = Guid.Parse(userId), Email = "test@example.com", ExternalId = userId, Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        // Tạo các gia đình.
        var managedFamily = new Family { Id = Guid.NewGuid(), Name = "Gia đình được quản lý", Code = "FAM003" };
        var unmanagedFamily = new Family { Id = Guid.NewGuid(), Name = "Gia đình không được quản lý", Code = "FAM004" };
        _context.Families.AddRange(managedFamily, unmanagedFamily);

        // Tạo FamilyUser để liên kết managedFamily với currentUserProfile.
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = managedFamily.Id, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và chỉ trả về gia đình được quản lý.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.Id.Should().Be(managedFamily.Id);
        result.Value!.First()!.Name.Should().Be("Gia đình được quản lý");
    }
}
