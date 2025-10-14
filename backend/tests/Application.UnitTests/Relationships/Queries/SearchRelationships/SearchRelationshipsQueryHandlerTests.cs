using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Queries.SearchRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.SearchRelationships;

public class SearchRelationshipsQueryHandlerTests : TestBase
{
    private readonly SearchRelationshipsQueryHandler _handler;

    public SearchRelationshipsQueryHandlerTests()
    {
        _handler = new SearchRelationshipsQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// Thiết lập môi trường kiểm thử bằng cách xóa dữ liệu cũ và tạo người dùng, hồ sơ người dùng, gia đình.
    /// </summary>
    /// <param name="userId">ID của người dùng hiện tại.</param>
    /// <param name="userProfileId">ID của hồ sơ người dùng.</param>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="isAdmin">Cho biết người dùng có phải là quản trị viên hay không.</param>
    /// <param name="canManageFamily">Cho biết người dùng có quyền quản lý gia đình hay không.</param>
    /// <param name="userProfileExists">Cho biết hồ sơ người dùng có tồn tại hay không.</param>
    private async Task ClearDatabaseAndSetupUser(string userId, Guid userProfileId, Guid familyId, bool isAdmin, bool canManageFamily, bool userProfileExists = true)
    {
        // Xóa tất cả dữ liệu liên quan để đảm bảo môi trường sạch cho mỗi bài kiểm tra.
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.Relationships.RemoveRange(_context.Relationships);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thiết lập ID người dùng hiện tại cho mock IUser.
        _mockUser.Setup(x => x.Id).Returns(userId);

        if (userProfileExists)
        {
            // Tạo và thêm hồ sơ người dùng vào cơ sở dữ liệu.
            var userProfile = new UserProfile { Id = userProfileId, ExternalId = userId, Email = "test@example.com", Name = "Test User" };
            _context.UserProfiles.Add(userProfile);
            // Thiết lập người dùng với vai trò Quản lý gia đình.
            _context.FamilyUsers.Add(new FamilyUser { FamilyId = familyId, UserProfileId = userProfileId, Role = FamilyRole.Manager });
            await _context.SaveChangesAsync(CancellationToken.None);

            // Thiết lập các hành vi của mock IAuthorizationService.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
            _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, userProfile)).Returns(canManageFamily);
        }
        else
        {
            // Thiết lập mock IAuthorizationService trả về null nếu hồ sơ người dùng không tồn tại.
            _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserProfile)null!);
        }
    }

    /// <summary>
    /// Kiểm tra tìm kiếm mối quan hệ theo từ khóa.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFilteredRelationships_WhenSearchTermApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "John", LastName = "Doe" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Jane", LastName = "Doe" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Peter", LastName = "Pan" };
        _context.Members.AddRange(member1, member2, member3);
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Husband });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member3.Id, Type = RelationshipType.Father });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchRelationshipsQuery { SourceMemberId = member1.Id, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2); // Both relationships involve John
    }

    /// <summary>
    /// Kiểm tra phân trang khi tìm kiếm mối quan hệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnPaginatedRelationships()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "2" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "3" };
        var member4 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "4" };
        var member5 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "5" };
        _context.Members.AddRange(member1, member2, member3, member4, member5);
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Husband });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member3.Id, Type = RelationshipType.Father });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member2.Id, TargetMemberId = member4.Id, Type = RelationshipType.Father });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member3.Id, TargetMemberId = member5.Id, Type = RelationshipType.Father });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member4.Id, TargetMemberId = member5.Id, Type = RelationshipType.Mother });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchRelationshipsQuery { ItemsPerPage = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2); // Handler trả về 2 mục cho trang đầu tiên
        result.Value.TotalItems.Should().Be(5);
        result.Value.Page.Should().Be(1);
        result.Value.TotalPages.Should().Be(3); // 5 items, 2 per page = 3 pages
    }
}
