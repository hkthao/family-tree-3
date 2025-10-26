using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.UpdateRelationship;

public class UpdateRelationshipCommandHandlerTests : TestBase
{
    private readonly UpdateRelationshipCommandHandler _handler;

    public UpdateRelationshipCommandHandlerTests()
    {
        _fixture.Customize(new AutoMoqCustomization());
        _handler = new UpdateRelationshipCommandHandler(
            _context,
            _mockAuthorizationService.Object
        );
    }



    [Fact]
    public async Task Handle_ShouldReturnFailureWhenRelationshipNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Đảm bảo mối quan hệ không tồn tại trong _context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Relationship with ID {command.Id} not found.");
        // 💡 Giải thích: Không thể cập nhật mối quan hệ không tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenSourceMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên nguồn của mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ vào _context, nhưng không thêm thành viên nguồn tương ứng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var relationship = _fixture.Build<Relationship>()
            .Without(r => r.SourceMember) // Ensure SourceMember is not loaded
            .Create();
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var command = new UpdateRelationshipCommand
        {
            Id = relationship.Id,
            SourceMemberId = relationship.SourceMemberId,
            TargetMemberId = relationship.TargetMemberId,
            Type = relationship.Type,
            Order = relationship.Order
        };

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
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền cập nhật mối quan hệ.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ và thành viên nguồn vào _context. Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();

        var familyId = Guid.NewGuid();
        var sourceMember = _fixture.Build<Member>()
            .With(m => m.FamilyId, familyId)
            .Create();
        var relationship = _fixture.Build<Relationship>()
            .With(r => r.SourceMemberId, sourceMember.Id)
            .With(r => r.SourceMember, sourceMember)
            .Create();
        _context.Members.Add(sourceMember);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId)).Returns(false);

        var command = new UpdateRelationshipCommand
        {
            Id = relationship.Id,
            SourceMemberId = relationship.SourceMemberId,
            TargetMemberId = relationship.TargetMemberId,
            Type = relationship.Type,
            Order = relationship.Order
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can update relationships.");
        result.ErrorSource.Should().Be("Forbidden");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình hoặc là admin để cập nhật mối quan hệ.
    }

    [Fact]
    public async Task Handle_ShouldUpdateRelationshipSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cập nhật mối quan hệ thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một mối quan hệ và thành viên nguồn vào _context. Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle với các thông tin cập nhật.
        // 3. Assert: Kiểm tra kết quả trả về là thành công. Xác minh mối quan hệ đã được cập nhật trong _context. Xác minh RecordActivityCommand được gửi.
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

        var targetMemberId = Guid.NewGuid();
        var targetMember = new Member
        {
            Id = targetMemberId,
            FamilyId = familyId,
            Code = "TM001",
            FirstName = "Target",
            LastName = "Member"
        };
        _context.Members.Add(targetMember);
        await _context.SaveChangesAsync();

        var relationshipId = Guid.NewGuid();
        var relationship = new Relationship
        {
            Id = relationshipId,
            SourceMemberId = sourceMember.Id,
            TargetMemberId = targetMember.Id,
            Type = RelationshipType.Father,
            Order = 1,
            FamilyId = familyId
        };
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId)).Returns(true);

        var updatedSourceMemberId = Guid.NewGuid();
        var updatedTargetMemberId = Guid.NewGuid();
        var updatedType = RelationshipType.Wife;
        var updatedOrder = 2;

        var command = new UpdateRelationshipCommand
        {
            Id = relationship.Id,
            SourceMemberId = updatedSourceMemberId,
            TargetMemberId = updatedTargetMemberId,
            Type = updatedType,
            Order = updatedOrder
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var updatedRelationship = await _context.Relationships.FindAsync(relationship.Id);
        updatedRelationship.Should().NotBeNull();
        updatedRelationship!.SourceMemberId.Should().Be(updatedSourceMemberId);
        updatedRelationship.TargetMemberId.Should().Be(updatedTargetMemberId);
        updatedRelationship.Type.Should().Be(updatedType);
        updatedRelationship.Order.Should().Be(updatedOrder);
        // 💡 Giải thích: Handler phải cập nhật mối quan hệ và ghi lại hoạt động.
    }
}
