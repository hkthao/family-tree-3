using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CreateFamiliesCommandHandler _handler;

    public CreateFamiliesCommandHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _handler = new CreateFamiliesCommandHandler(_context, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleFamiliesAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var familiesToCreate = new List<FamilyDto>
        {
            new() { Name = "Family 1", Description = "Desc 1", Address = "Addr 1", Visibility = "Public", Code = "F1" },
            new() { Name = "Family 2", Description = "Desc 2", Address = "Addr 2", Visibility = "Private", Code = "F2" }
        };
        var command = new CreateFamiliesCommand(familiesToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdFamilies = await _context.Families.Include(f => f.FamilyUsers).ToListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        createdFamilies.Should().HaveCount(2);

        createdFamilies.Should().Contain(f => f.Name == "Family 1" && f.FamilyUsers.Any(fu => fu.UserId == userId && fu.Role == FamilyRole.Manager));
        createdFamilies.Should().Contain(f => f.Name == "Family 2" && f.FamilyUsers.Any(fu => fu.UserId == userId && fu.Role == FamilyRole.Manager));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_OnDbUpdateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var familiesToCreate = new List<FamilyDto>
        {
            new() { Name = "Family with error", Visibility = "Public" }
        };
        var command = new CreateFamiliesCommand(familiesToCreate);

        // Simulate a DbUpdateException
        var mockedContext = new Mock<IApplicationDbContext>();
        mockedContext.Setup(c => c.Families.Add(It.IsAny<Family>()));
        mockedContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Simulated DB update error", new Exception()));

        var handlerWithMockedContext = new CreateFamiliesCommandHandler(mockedContext.Object, _currentUserMock.Object);

        // Act
        var result = await handlerWithMockedContext.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Simulated DB update error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_OnGenericException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var familiesToCreate = new List<FamilyDto>
        {
            new() { Name = "Family with error", Visibility = "Public" }
        };
        var command = new CreateFamiliesCommand(familiesToCreate);

        var mockedContext = new Mock<IApplicationDbContext>();
        mockedContext.Setup(c => c.Families.Add(It.IsAny<Family>()))
            .Throws(new Exception("Simulated generic error"));
        mockedContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handlerWithMockedContext = new CreateFamiliesCommandHandler(mockedContext.Object, _currentUserMock.Object);

        // Act
        var result = await handlerWithMockedContext.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Exception);
        result.Error.Should().Contain("Simulated generic error");
    }
}