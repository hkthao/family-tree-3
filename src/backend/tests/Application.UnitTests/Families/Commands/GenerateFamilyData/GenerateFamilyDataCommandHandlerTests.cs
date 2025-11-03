using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using backend.Application.Families;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IChatProviderFactory> _chatProviderFactoryMock;
    private readonly Mock<IChatProvider> _chatProviderMock;
    private readonly Mock<IValidator<FamilyDto>> _familyDtoValidatorMock;

    public GenerateFamilyDataCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _chatProviderFactoryMock = new Mock<IChatProviderFactory>();
        _chatProviderMock = new Mock<IChatProvider>();
        _familyDtoValidatorMock = new Mock<IValidator<FamilyDto>>();

        _chatProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<ChatAIProvider>())).Returns(_chatProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateFamilyData_WhenValidCommandAndUserIsAdmin()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);
        _authorizationServiceMock.Setup(x => x.CanManageFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Family for Data Generation",
            Code = "GEN001",
            Description = "Description",
            Address = "Address",
            Visibility = "Private"
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var prompt = "Generate a family named 'Test Family' with 5 members and 2 generations.";
        var command = new GenerateFamilyDataCommand(prompt);

        var aiResponseJson = "{\"families\":[{\"name\":\"Test Family\",\"description\":\"A test family\",\"address\":\"Test Address\",\"visibility\":\"Public\",\"avatarUrl\":null,\"totalMembers\":5,\"totalGenerations\":2}]}";
        _chatProviderMock.Setup(x => x.GenerateResponseAsync(It.IsAny<List<ChatMessage>>())).ReturnsAsync(aiResponseJson);

        _familyDtoValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<FamilyDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GenerateFamilyDataCommandHandler(_chatProviderFactoryMock.Object, _familyDtoValidatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be("Test Family");
        result.Value!.First().TotalMembers.Should().Be(5);
        result.Value!.First().TotalGenerations.Should().Be(2);
    }
}
