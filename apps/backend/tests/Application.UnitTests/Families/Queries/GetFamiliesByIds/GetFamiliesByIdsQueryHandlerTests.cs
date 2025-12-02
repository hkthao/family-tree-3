using AutoMapper;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandlerTests : TestBase
{
    public GetFamiliesByIdsQueryHandlerTests()
    {
        // Set up authenticated user by default for most tests
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        // Default to non-admin for most tests, override in specific admin tests
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFamilies_WhenGivenValidIds()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var authenticatedUserId = Guid.NewGuid();
        _mockUser.Setup(c => c.UserId).Returns(authenticatedUserId);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1", CreatedBy = authenticatedUserId.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2", CreatedBy = authenticatedUserId.ToString() };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3", CreatedBy = authenticatedUserId.ToString() };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var handler = new GetFamiliesByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new GetFamiliesByIdsQuery(new List<Guid> { family1.Id, family3.Id });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // Corrected assertion
        result.Value!.Select(f => f.Id).Should().Contain(family1.Id);
        result.Value!.Select(f => f.Id).Should().Contain(family3.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenGivenNoIds()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var handler = new GetFamiliesByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new GetFamiliesByIdsQuery(new List<Guid>());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenGivenNonExistentIds()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        var handler = new GetFamiliesByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
        var query = new GetFamiliesByIdsQuery(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFamilies_WhenUserIsAdmin()
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

        var handler = new GetFamiliesByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Admin Family 1", Code = "ADM1", CreatedBy = Guid.NewGuid().ToString(), Visibility = FamilyVisibility.Private.ToString() };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Admin Family 2", Code = "ADM2", CreatedBy = Guid.NewGuid().ToString(), Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new GetFamiliesByIdsQuery(new List<Guid> { family1.Id, family2.Id });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // Corrected assertion
        result.Value!.Select(f => f.Id).Should().Contain(family1.Id);
        result.Value!.Select(f => f.Id).Should().Contain(family2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserIsUnauthenticated()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.Families.RemoveRange(_context.Families);
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        await _context.SaveChangesAsync();

        _mockUser.Setup(c => c.UserId).Returns(Guid.Empty);
        _mockUser.Setup(c => c.IsAuthenticated).Returns(false);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);

        var handler = new GetFamiliesByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PVT1", CreatedBy = Guid.NewGuid().ToString(), Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.Add(family1);
        await _context.SaveChangesAsync();

        var query = new GetFamiliesByIdsQuery(new List<Guid> { family1.Id });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Should return success with empty list
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
