using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public CreateMemberCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có tạo thành công một thành viên mới khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateMember_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 0,
            TotalGenerations = 0
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            Code = "JD001",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new CreateMemberCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var member = await handlerContext.Members.FindAsync(result.Value);
        member.Should().NotBeNull();
        member!.FamilyId.Should().Be(command.FamilyId);
        member.FirstName.Should().Be(command.FirstName);
        member.LastName.Should().Be(command.LastName);
        member.Code.Should().Be(command.Code);
        member.Gender.Should().Be(command.Gender);
        member.DateOfBirth.Should().Be(command.DateOfBirth);
        member.IsRoot.Should().Be(command.IsRoot);
    }
}
