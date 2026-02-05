using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Services;
using backend.Application.MemoryItems.DTOs; // For MemoryItemDto
using backend.Application.MemoryItems.Queries.GetMemoryItemDetail;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemoryItems.Queries;

public class GetMemoryItemDetailQueryHandlerTests : TestBase
{
    private readonly GetMemoryItemDetailQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetMemoryItemDetailQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<MemoryItemDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemoryItemDto dto, Guid familyId, CancellationToken token) => dto);

        _handler = new GetMemoryItemDetailQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object, _mockPrivacyService.Object);
    }

    // Helper method to create a family and a memory item for tests
    private async Task<(Family family, MemoryItem memoryItem)> CreateFamilyAndMemoryItem(Guid ownerUserId)
    {
        var family = Family.Create("Test Family", "TF001", null, null, "Private", ownerUserId);
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var memoryItem = new MemoryItem(family.Id, "Test Memory", "Description");
        _context.MemoryItems.Add(memoryItem);
        await _context.SaveChangesAsync();

        return (family, memoryItem);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemoryItemDetail_WhenUserCanAccessFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Not admin
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true); // Can access

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(userId);

        var query = new GetMemoryItemDetailQuery { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memoryItem.Id);
        result.Value.Title.Should().Be(memoryItem.Title);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemoryItemDetail_WhenUserIsAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Admin

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid()); // Family owned by another user

        var query = new GetMemoryItemDetailQuery { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memoryItem.Id);
        result.Value.Title.Should().Be(memoryItem.Title);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _mockUser.Setup(x => x.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false);

        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid());

        var query = new GetMemoryItemDetailQuery { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        result.ErrorSource.Should().Be(ErrorSources.Authentication);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserCannotAccessAndIsNotAdmin()
    {
        // Arrange
        var (family, memoryItem) = await CreateFamilyAndMemoryItem(Guid.NewGuid()); // Family owned by another user

        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Not admin
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(false); // Cannot access

        var query = new GetMemoryItemDetailQuery { Id = memoryItem.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Forbidden");
        result.ErrorSource.Should().Be("Authorization");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemoryItemDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(x => x.UserId).Returns(userId);
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Admin, so access is not an issue

        var query = new GetMemoryItemDetailQuery { Id = Guid.NewGuid() }; // Non-existent ID

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("NotFound");
    }
}
