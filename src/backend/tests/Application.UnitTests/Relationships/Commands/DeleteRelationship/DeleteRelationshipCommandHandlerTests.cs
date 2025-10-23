using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.DeleteRelationship;

public class DeleteRelationshipCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly DeleteRelationshipCommandHandler _handler;

    public DeleteRelationshipCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new DeleteRelationshipCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi hồ sơ người dùng không tìm thấy.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về null.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var command = new DeleteRelationshipCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể xóa mối quan hệ nếu không tìm thấy hồ sơ người dùng hiện tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenRelationshipNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Đảm bảo mối quan hệ không tồn tại trong _context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        var command = new DeleteRelationshipCommand(Guid.NewGuid()); // Non-existent ID

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Relationship with ID {command.Id} not found.");
        // 💡 Giải thích: Không thể xóa mối quan hệ không tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenSourceMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên nguồn của mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ vào _context, nhưng không thêm thành viên nguồn tương ứng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        var relationship = _fixture.Build<Relationship>()
            .Without(r => r.SourceMember) // Ensure SourceMember is not loaded
            .Create();
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var command = new DeleteRelationshipCommand(relationship.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Source member for relationship {command.Id} not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể xác thực quyền nếu không tìm thấy thành viên nguồn.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserNotAuthorized()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền xóa mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ và thành viên nguồn vào _context. Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

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
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId, currentUserProfile)).Returns(false);

        var command = new DeleteRelationshipCommand(relationship.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can delete relationships.");
        result.ErrorSource.Should().Be("Forbidden");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình hoặc là admin để xóa mối quan hệ.
    }

    [Fact]
    public async Task Handle_ShouldDeleteRelationshipSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler xóa mối quan hệ thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ và thành viên nguồn vào _context. Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

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
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId, currentUserProfile)).Returns(true);

        var command = new DeleteRelationshipCommand(relationship.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        // 💡 Giải thích: Handler phải xóa mối quan hệ và ghi lại hoạt động.
    }
}
