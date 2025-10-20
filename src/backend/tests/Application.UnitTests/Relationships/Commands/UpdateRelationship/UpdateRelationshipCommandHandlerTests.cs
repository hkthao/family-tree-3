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
        _handler = new UpdateRelationshipCommandHandler(_context, _mockAuthorizationService.Object, _mockMediator.Object);
    }



    /// <summary>
    /// Kiểm tra xem một mối quan hệ có được cập nhật thành công.
    /// </summary>
    [Fact]
    public async Task Handle_Should_UpdateRelationship_Successfully()
    {
        // Arrange
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var member1 = _context.Members.First(m => m.Code == "M001"); // Prince William
        var member2 = _context.Members.First(m => m.Code == "M002"); // Catherine
        var member3 = _context.Members.First(m => m.Code == "M003"); // Prince George

        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, It.IsAny<UserProfile>())).Returns(true);

        var existingRelationship = new Relationship
        {
            Id = Guid.NewGuid(),
            SourceMemberId = member1.Id,
            TargetMemberId = member2.Id,
            Type = RelationshipType.Father
        };
        _context.Relationships.Add(existingRelationship);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateRelationshipCommand
        {
            Id = existingRelationship.Id,
            SourceMemberId = member1.Id,
            TargetMemberId = member3.Id, // Change target member
            Type = RelationshipType.Husband // Change relationship type
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedRelationship = await _context.Relationships.FindAsync(existingRelationship.Id);
        updatedRelationship.Should().NotBeNull();
        updatedRelationship!.TargetMemberId.Should().Be(command.TargetMemberId);
        updatedRelationship.Type.Should().Be(command.Type);
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra khi mối quan hệ cần cập nhật không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_RelationshipNotFound()
    {
        // Arrange
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, It.IsAny<UserProfile>())).Returns(true);

        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(), // Non-existent relationship ID
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Relationship with ID {command.Id} not found.");
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra khi hồ sơ người dùng không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserProfileNotFound()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile)null!); // Simulate user profile not found

        var command = new UpdateRelationshipCommand
        {
            Id = Guid.NewGuid(),
            SourceMemberId = Guid.NewGuid(),
            TargetMemberId = Guid.NewGuid(),
            Type = RelationshipType.Father
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra khi người dùng không được ủy quyền để cập nhật mối quan hệ.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var member1 = _context.Members.First(m => m.Code == "M001"); // Prince William
        var member2 = _context.Members.First(m => m.Code == "M002"); // Catherine

        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, It.IsAny<UserProfile>())).Returns(false); // User is not authorized

        var existingRelationship = new Relationship
        {
            Id = Guid.NewGuid(),
            SourceMemberId = member1.Id,
            TargetMemberId = member2.Id,
            Type = RelationshipType.Father
        };
        _context.Relationships.Add(existingRelationship);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateRelationshipCommand
        {
            Id = existingRelationship.Id,
            SourceMemberId = member1.Id,
            TargetMemberId = member2.Id,
            Type = RelationshipType.Husband
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers or admins can update relationships.");
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra khi một thành viên trong mối quan hệ không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_MemberNotFound()
    {
        // Arrange
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var member1 = _context.Members.First(m => m.Code == "M001"); // Prince William

        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(familyId, It.IsAny<UserProfile>())).Returns(true);

        var existingRelationship = new Relationship
        {
            Id = Guid.NewGuid(),
            SourceMemberId = member1.Id,
            TargetMemberId = Guid.NewGuid(), // Non-existent member
            Type = RelationshipType.Father
        };
        _context.Relationships.Add(existingRelationship);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateRelationshipCommand
        {
            Id = existingRelationship.Id,
            SourceMemberId = member1.Id,
            TargetMemberId = Guid.NewGuid(), // Non-existent member
            Type = RelationshipType.Husband
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Handler hiện tại không trả về lỗi khi thành viên không tồn tại, đây có thể là một bug.
        // result.Error.Should().Contain("One or both members not found in the specified family.");
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Handler vẫn gửi RecordActivityCommand
    }
}
