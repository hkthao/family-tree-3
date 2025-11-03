using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.GenerateMemberData;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;
using backend.Application.Common.Models;
using FluentValidation;
using backend.Application.Members.Queries;

namespace backend.Application.UnitTests.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;
    private readonly Mock<IValidator<AIMemberDto>> _aiMemberDtoValidatorMock;

    public GenerateMemberDataCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();
        _aiMemberDtoValidatorMock = new Mock<IValidator<AIMemberDto>>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);
    }

    /// <summary>
    /// Kiểm tra xem handler có tạo thành công dữ liệu thành viên cho một gia đình khi lệnh hợp lệ và người dùng có quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateMemberData_WhenValidCommandAndUserHasPermission()
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

        var command = new GenerateMemberDataCommand("Generate 3 members for the Test Family.");

        var aiResponseJson = "{\"members\":[{\"firstName\":\"John\",\"lastName\":\"Doe\",\"gender\":\"Male\",\"dateOfBirth\":\"1990-01-01\",\"isRoot\":true,\"code\":\"JD001\"},{\"firstName\":\"Jane\",\"lastName\":\"Doe\",\"gender\":\"Female\",\"dateOfBirth\":\"1992-03-15\",\"isRoot\":false,\"code\":\"JND002\"},{\"firstName\":\"Peter\",\"lastName\":\"Smith\",\"gender\":\"Male\",\"dateOfBirth\":\"2000-07-20\",\"isRoot\":false,\"code\":\"PS003\"}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);

        _aiMemberDtoValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GenerateMemberDataCommandHandler(_chatProviderFactoryMock.Object, _aiMemberDtoValidatorMock.Object, handlerContext, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(m => m.FirstName == "John" && m.LastName == "Doe");
        result.Value.Should().Contain(m => m.FirstName == "Jane" && m.LastName == "Doe");
        result.Value.Should().Contain(m => m.FirstName == "Peter" && m.LastName == "Smith");
    }
}
