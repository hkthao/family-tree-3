using Moq;
using backend.Application.Common.Constants;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Families.Dtos; // For FamilyUserDto

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetFamilyByIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyDetailDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyDetailDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<FamilyDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<FamilyDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FamilyDto> dtos, Guid familyId, CancellationToken token) => dtos);

        // TestBase already sets up _mockUser and _mockAuthorizationService
        // Set default authenticated user for specific scenarios if needed
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        // Default to non-admin for most tests, override in specific admin tests
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamily_WhenFamilyExists()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = _mockUser.Object.UserId; // Use mocked authenticated user
        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.Add(testFamily);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(testFamily.Id);
        var handler = new GetFamilyByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testFamily.Id);
        result.Value.Name.Should().Be(testFamily.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(Guid.NewGuid());
        var handler = new GetFamilyByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, query.Id));
    }

    [Fact]
    public async Task Handle_ShouldReturnFamily_WhenUserIsAdmin()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var adminUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(adminUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Simulate admin user

        var handler = new GetFamilyByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);

        var testFamily = new Family { Id = Guid.NewGuid(), Name = "Admin Family", Code = "ADM1", CreatedBy = Guid.NewGuid().ToString(), Visibility = "Private" };
        _context.Families.Add(testFamily);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(testFamily.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testFamily.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        _mockUser.Setup(c => c.UserId).Returns(Guid.Empty);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var handler = new GetFamilyByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);

        var privateFamily = new Family { Id = Guid.NewGuid(), Name = "Private Family", Code = "PVT1", CreatedBy = Guid.NewGuid().ToString(), Visibility = "Private" };
        _context.Families.Add(privateFamily);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(privateFamily.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.Unauthorized);
    }
}
