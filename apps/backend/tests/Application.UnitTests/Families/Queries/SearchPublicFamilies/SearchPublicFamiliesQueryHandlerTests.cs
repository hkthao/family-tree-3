using backend.Application.Families.Queries.SearchPublicFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchPublicFamilies;

public class SearchPublicFamiliesQueryHandlerTests : TestBase
{
    private readonly SearchPublicFamiliesQueryHandler _handler;

    public SearchPublicFamiliesQueryHandlerTests()
    {
        _handler = new SearchPublicFamiliesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyPublicFamilies()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family 1", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family 2", Code = "PUB2", Visibility = FamilyVisibility.Public.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredPublicFamilies_WhenSearchTermProvided()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Alpha", Code = "PUB_A", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Beta", Code = "PUB_B", Visibility = FamilyVisibility.Public.ToString() });
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family Gamma", Code = "PRIV_G", Visibility = FamilyVisibility.Private.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { SearchTerm = "Alpha" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Public Family Alpha");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPublicFamiliesExist()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Private Family 1", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatchingPublicFamilies()
    {
        // Arrange
        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Public Family Alpha", Code = "PUB_A", Visibility = FamilyVisibility.Public.ToString() });
        await _context.SaveChangesAsync();

        var query = new SearchPublicFamiliesQuery { SearchTerm = "NonExistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }
}
