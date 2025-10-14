using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries.GetRelationships;

public class GetRelationshipsQueryHandlerTests : TestBase
{
    private readonly GetRelationshipsQueryHandler _handler;

    public GetRelationshipsQueryHandlerTests()
    {
        _handler = new GetRelationshipsQueryHandler(_context, _mapper);
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
    /// Kiểm tra xem tất cả các mối quan hệ có được trả về khi không có bộ lọc nào được áp dụng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllRelationships_WhenNoFilterIsApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member", LastName = "2" };
        _context.Members.AddRange(member1, member2);
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member1.Id, TargetMemberId = member2.Id, Type = RelationshipType.Father });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMemberId = member2.Id, TargetMemberId = member1.Id, Type = RelationshipType.Mother });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetRelationshipsQuery { FamilyId = familyId, ItemsPerPage = 1000 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Kiểm tra khi không có mối quan hệ nào cho FamilyId được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRelationshipsForFamilyId()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetRelationshipsQuery { FamilyId = familyId, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }
}
