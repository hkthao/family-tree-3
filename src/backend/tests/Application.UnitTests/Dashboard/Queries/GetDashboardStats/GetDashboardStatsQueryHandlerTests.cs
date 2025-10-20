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

    public GetDashboardStatsQueryHandlerTests()
    {
        _handler = new GetDashboardStatsQueryHandler(_context, _mockAuthorizationService.Object);
    }

    private UserProfile SetupUserAuthorization(bool isAdmin)
    {
        var userProfileId = Guid.NewGuid();
        var currentUserProfile = new UserProfile { Id = userProfileId, ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(currentUserProfile);
        _context.SaveChanges();

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
        return currentUserProfile;
    }



    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê chính xác khi có dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenDataExists()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        // Ensure the current user is a manager of the royal family
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"), UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery();
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1);
        result.Value.TotalMembers.Should().Be(19);
        result.Value.TotalRelationships.Should().Be(0);
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê bằng 0 khi không có dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnZeroStats_WhenNoDataExists()
    {
        // Arrange
        SetupUserAuthorization(false);

        // Clear all data to ensure no data exists
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Relationships.RemoveRange(_context.Relationships);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(0);
        result.Value.TotalMembers.Should().Be(0);
        result.Value.TotalRelationships.Should().Be(0);
        result.Value.TotalGenerations.Should().Be(0);
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về lỗi khi không tìm thấy hồ sơ người dùng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile)null!);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê chính xác cho người dùng quản trị viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenUserIsAdmin()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(true); // User is admin
        // Ensure the current user is a manager of the royal family
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"), UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);
        var query = new GetDashboardStatsQuery();
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1); // Admin sees both families
        result.Value.TotalMembers.Should().Be(19);
        result.Value.TotalRelationships.Should().Be(0);
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê chính xác khi FamilyId được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenFamilyIdIsProvided()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        // Ensure the current user is a manager of the royal family
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery { FamilyId = royalFamilyId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1); // Only family1
        result.Value.TotalMembers.Should().Be(19); // Members of family1
        result.Value.TotalRelationships.Should().Be(0); // Relationships where SourceMember is from family1
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }
}