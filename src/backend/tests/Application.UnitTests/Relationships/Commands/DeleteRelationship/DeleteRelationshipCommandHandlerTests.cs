using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Constants;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

/// <summary>
/// Bộ test cho DeleteRelationshipCommandHandler.
/// </summary>
public class DeleteRelationshipCommandHandlerTests : TestBase
{

    private readonly DeleteRelationshipCommandHandler _handler;

    public DeleteRelationshipCommandHandlerTests()
    {

        _fixture.Customize(new AutoMoqCustomization());

        _handler = new DeleteRelationshipCommandHandler(
            _context,
            _mockAuthorizationService.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy mối quan hệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một DeleteRelationshipCommand với Id không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.NotFound
    ///              và ErrorSource là ErrorSources.NotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Không thể xóa mối quan hệ không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenRelationshipNotFound()
    {
        // Arrange
        var command = new DeleteRelationshipCommand(Guid.NewGuid()); // Non-existent ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Relationship with ID {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên nguồn của mối quan hệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một mối quan hệ vào _context, nhưng không thêm thành viên nguồn tương ứng.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.NotFound
    ///              và ErrorSource là ErrorSources.NotFound.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Không thể xác thực quyền nếu không tìm thấy thành viên nguồn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenSourceMemberNotFound()
    {
        // Arrange
        var relationship = _fixture.Build<Relationship>()
            .Without(r => r.SourceMember) // Ensure SourceMember is not loaded
            .Create();
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var command = new DeleteRelationshipCommand(relationship.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Source member for relationship {command.Id}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền xóa mối quan hệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về false.
    ///               Thêm một mối quan hệ và thành viên nguồn vào _context.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.AccessDenied
    ///              và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng phải có quyền quản lý gia đình hoặc là admin để xóa mối quan hệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var sourceMember = _fixture.Build<Member>()
            .With(m => m.FamilyId, familyId)
            .Create();
        var relationship = _fixture.Build<Relationship>()
            .With(r => r.SourceMemberId, sourceMember.Id)
            .Create();
        _context.Members.Add(sourceMember);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId)).Returns(false);

        var command = new DeleteRelationshipCommand(relationship.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRelationshipSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa mối quan hệ thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ và thành viên nguồn vào _context. Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle.
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var sourceMemberId = Guid.NewGuid();
        var sourceMember = new Member
        {
            Id = sourceMemberId,
            FamilyId = familyId,
            Code = "SM001",
            FirstName = "Source",
            LastName = "Member"
        };
        _context.Members.Add(sourceMember);
        await _context.SaveChangesAsync();

        var relationshipId = Guid.NewGuid();
        var relationship = new Relationship
        {
            Id = relationshipId,
            SourceMemberId = sourceMember.Id,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            FamilyId = familyId
        }; _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId)).Returns(true);

        var command = new DeleteRelationshipCommand(relationship.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // 💡 Giải thích: Handler phải xóa mối quan hệ và ghi lại hoạt động.
    }
}