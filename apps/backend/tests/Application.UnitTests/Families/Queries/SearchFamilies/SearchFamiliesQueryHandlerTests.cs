using AutoMapper;
using backend.Application.Families;
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly SearchFamiliesQueryHandler _handler;

    public SearchFamiliesQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handler = new SearchFamiliesQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedListOfFamilies_WhenCalled()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3" };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 2 };

        // Setup mapper to return FamilyDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Items.First().Name.Should().Be("Family 1");
        result.Value.Items.Last().Name.Should().Be("Family 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenSearchQueryIsProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family Alpha", Description = "Description for Alpha", Code = "FA" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family Beta", Description = "Description for Beta", Code = "FB" };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { SearchQuery = "Alpha", Page = 1, ItemsPerPage = 10 };

        // Setup mapper to return FamilyDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Family Alpha");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenVisibilityIsProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family Public", Visibility = "Public", Code = "FP" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family Private", Visibility = "Private", Code = "FPR" };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { Visibility = "Public", Page = 1, ItemsPerPage = 10 };

        // Setup mapper to return FamilyDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Family Public");
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderedFamilies_WhenSortByAndSortOrderAreProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family C", Code = "FC" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB" };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { SortBy = "Name", SortOrder = "asc", Page = 1, ItemsPerPage = 10 };

        // Setup mapper to return FamilyDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items.First().Name.Should().Be("Family A");
        result.Value.Items.Last().Name.Should().Be("Family C");
    }
}
