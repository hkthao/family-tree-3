using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Commands.CreateRelationship;

public class CreateRelationshipCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CreateRelationshipCommandHandler _handler;

    public CreateRelationshipCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();
        _fixture.Customize(new AutoMoqCustomization());

        _handler = new CreateRelationshipCommandHandler(
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

        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể tạo mối quan hệ nếu không tìm thấy hồ sơ người dùng hiện tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenSourceMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên nguồn.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Đảm bảo thành viên nguồn không tồn tại trong Context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        var command = new CreateRelationshipCommand
        {
            SourceMemberId = Guid.NewGuid(), // Non-existent ID
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Source member with ID {command.SourceMemberId} not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Không thể tạo mối quan hệ nếu thành viên nguồn không tồn tại.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserNotAuthorized()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không được ủy quyền quản lý gia đình của thành viên nguồn.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm một thành viên nguồn vào Context.
        //             Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var currentUserProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        var familyId = Guid.NewGuid();
        var sourceMember = _fixture.Build<Member>()
            .With(m => m.FamilyId, familyId)
            .Create();
        _context.Members.Add(sourceMember);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId, currentUserProfile)).Returns(false);

        var command = new CreateRelationshipCommand
        {
            SourceMemberId = sourceMember.Id,
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can create relationships.");
        result.ErrorSource.Should().Be("Forbidden");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình để tạo mối quan hệ.
    }

    [Fact]
    public async Task Handle_ShouldCreateRelationshipSuccessfully()
    {
        // 🎯 Mục tiêu của test: Xác minh handler tạo mối quan hệ thành công.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thiết lập _mockAuthorizationService.GetCurrentUserProfileAsync trả về một UserProfile hợp lệ. Thêm thành viên nguồn và đích vào Context.
        //             Thiết lập _mockAuthorizationService.IsAdmin trả về false và _mockAuthorizationService.CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa Id của mối quan hệ mới. Xác minh mối quan hệ mới được thêm vào Context.
        var currentUserProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User"
        };
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);

        var familyId = Guid.NewGuid();
        var sourceMember = new Member
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Code = "SM001",
            FirstName = "Source",
            LastName = "Member"
        };
        var targetMember = new Member
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Code = "TM001",
            FirstName = "Target",
            LastName = "Member"
        };
        _context.Members.AddRange(sourceMember, targetMember);
        await _context.SaveChangesAsync();

        var retrievedSourceMember = await _context.Members.FindAsync(sourceMember.Id);
        retrievedSourceMember.Should().NotBeNull(); // Ensure source member is in DB

        var retrievedTargetMember = await _context.Members.FindAsync(targetMember.Id);
        retrievedTargetMember.Should().NotBeNull(); // Ensure target member is in DB

        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(familyId, It.IsAny<UserProfile>())).Returns(true);

        var command = new CreateRelationshipCommand
        {
            SourceMemberId = retrievedSourceMember!.Id,
            TargetMemberId = retrievedTargetMember!.Id,
            Type = RelationshipType.Father,
            Order = 1
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        _mockAuthorizationService.Verify(s => s.IsAdmin(), Times.Once);
        _mockAuthorizationService.Verify(s => s.CanManageFamily(familyId, currentUserProfile), Times.Once);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _context.Relationships.Should().ContainSingle(r => r.Id == result.Value);
        var newRelationship = _context.Relationships.First(r => r.Id == result.Value);
        newRelationship.SourceMemberId.Should().Be(sourceMember.Id);
        newRelationship.TargetMemberId.Should().Be(targetMember.Id);
        newRelationship.Type.Should().Be(RelationshipType.Father);
        newRelationship.Order.Should().Be(1);

        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        // 💡 Giải thích: Handler phải tạo một mối quan hệ mới với các thuộc tính được cung cấp và ghi lại hoạt động.
    }
}
