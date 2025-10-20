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

    private async Task<(UserProfile currentUserProfile, Family family1, Family family2, Member member1, Member member2, Member member3)> SetupTestData(Guid userProfileId, bool isAdmin, CancellationToken cancellationToken)
    {
        var currentUserProfile = new UserProfile { Id = userProfileId, ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", CreatedBy = currentUserProfile.ExternalId, Code = "FAM001" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", CreatedBy = currentUserProfile.ExternalId, Code = "FAM002" };
        _context.Families.Add(family1);
        _context.Families.Add(family2);

        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = userProfileId });
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family2.Id, UserProfileId = userProfileId });

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "1", Code = "MEM001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "2", Code = "MEM002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member", LastName = "3", Code = "MEM003" };
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.Members.Add(member3);

        await _context.SaveChangesAsync(cancellationToken);

        return (currentUserProfile, family1, family2, member1, member2, member3);
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê chính xác khi có dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenDataExists()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var (_, family1, _, member1, member2, member3) = await SetupTestData(userProfileId, false, CancellationToken.None);

        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member1, TargetMember = member2, Type = RelationshipType.Father, FamilyId = family1.Id });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member2, TargetMember = member3, Type = RelationshipType.Mother, FamilyId = family1.Id });

        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2);
        result.Value.TotalMembers.Should().Be(3);
        result.Value.TotalRelationships.Should().Be(2);
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê bằng 0 khi không có dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnZeroStats_WhenNoDataExists()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var currentUserProfile = new UserProfile { Id = userProfileId, ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        // Không thêm bất kỳ dữ liệu nào vào cơ sở dữ liệu

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
        var userProfileId = Guid.NewGuid();
        var (_, _, _, member1, member2, member3) = await SetupTestData(userProfileId, true, CancellationToken.None); // User is admin

        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member1, TargetMember = member2, Type = RelationshipType.Father });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member2, TargetMember = member3, Type = RelationshipType.Mother });

        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(2); // Admin sees both families
        result.Value.TotalMembers.Should().Be(3);
        result.Value.TotalRelationships.Should().Be(2);
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }

    /// <summary>
    /// Kiểm tra xem GetDashboardStatsQueryHandler có trả về số liệu thống kê chính xác khi FamilyId được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectStats_WhenFamilyIdIsProvided()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var (_, family1, _, member1, member2, member3) = await SetupTestData(userProfileId, false, CancellationToken.None);

        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member1, TargetMember = member2, Type = RelationshipType.Father, FamilyId = family1.Id });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member2, TargetMember = member3, Type = RelationshipType.Mother, FamilyId = family1.Id });

        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetDashboardStatsQuery { FamilyId = family1.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFamilies.Should().Be(1); // Only family1
        result.Value.TotalMembers.Should().Be(2); // Members of family1
        result.Value.TotalRelationships.Should().Be(2); // Relationships where SourceMember is from family1
        result.Value.TotalGenerations.Should().Be(0); // Placeholder in handler
    }
}