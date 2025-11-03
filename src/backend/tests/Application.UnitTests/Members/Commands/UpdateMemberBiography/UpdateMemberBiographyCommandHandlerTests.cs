using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMemberBiography;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Commands.UpdateMemberBiography;

public class UpdateMemberBiographyCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public UpdateMemberBiographyCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có cập nhật tiểu sử thành công khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateBiography_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var newBiographyContent = "This is a new biography content.";

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001"
        };

        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Biography = "Old biography content."
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new UpdateMemberBiographyCommand
        {
            MemberId = memberId,
            BiographyContent = newBiographyContent
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new UpdateMemberBiographyCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedMember = await handlerContext.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember!.Biography.Should().Be(newBiographyContent);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi NotFound khi không tìm thấy thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = Guid.NewGuid(); // Member này sẽ không tồn tại
        var newBiographyContent = "This is a new biography content.";

        var command = new UpdateMemberBiographyCommand
        {
            MemberId = memberId,
            BiographyContent = newBiographyContent
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new UpdateMemberBiographyCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {memberId} not found.");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi AccessDenied khi người dùng không có quyền truy cập gia đình của thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserLacksPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var newBiographyContent = "This is a new biography content.";

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false); // User lacks permission

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001"
        };

        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Biography = "Old biography content."
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new UpdateMemberBiographyCommand
        {
            MemberId = memberId,
            BiographyContent = newBiographyContent
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new UpdateMemberBiographyCommandHandler(handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }
}
