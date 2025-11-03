using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.GenerateBiography;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;
using backend.Application.Common.Models;

namespace backend.Application.UnitTests.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;

    public GenerateBiographyCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);
    }

    /// <summary>
    /// Kiểm tra xem handler có tạo thành công tiểu sử cho một thành viên khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateBiography_WhenValidCommandAndUserHasPermission()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            TotalMembers = 1,
            TotalGenerations = 1
        };

        var existingMember = new Member("Doe", "John", "JD001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsRoot = true
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Prompt = "Generate a biography for John Doe.",
            Tone = BiographyTone.Neutral,
            UseSystemData = true
        };

        var generatedBiography = "John Doe was born on January 1, 1990. He is a male member of the Test Family.";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(generatedBiography);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GenerateBiographyCommandHandler(handlerContext, _authorizationServiceMock.Object, _chatProviderFactoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().Be(generatedBiography);

        var updatedMember = await handlerContext.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember!.Biography.Should().Be(generatedBiography);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi khi không tìm thấy thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid(); // MemberId không tồn tại
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Prompt = "Generate a biography for a non-existent member.",
            Tone = BiographyTone.Neutral,
            UseSystemData = true
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GenerateBiographyCommandHandler(handlerContext, _authorizationServiceMock.Object, _chatProviderFactoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Should().Contain($"Member with ID {memberId} not found.");
    }
}
