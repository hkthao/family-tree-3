using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public UpdateMemberCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có cập nhật thành công một thành viên hiện có khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateMember_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 1,
            TotalGenerations = 1
        };

        var existingMember = new Member("OldLastName", "OldFirstName", "OLD001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FamilyId = familyId,
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            Code = "NEW001",
            Gender = "Female",
            DateOfBirth = new DateTime(1995, 5, 5),
            IsRoot = false
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new UpdateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedMember = await handlerContext.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember!.FamilyId.Should().Be(command.FamilyId);
        updatedMember.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.Code.Should().Be(command.Code);
        updatedMember.Gender.Should().Be(command.Gender);
        updatedMember.DateOfBirth.Should().Be(command.DateOfBirth);
        updatedMember.IsRoot.Should().Be(command.IsRoot);
    }
}
