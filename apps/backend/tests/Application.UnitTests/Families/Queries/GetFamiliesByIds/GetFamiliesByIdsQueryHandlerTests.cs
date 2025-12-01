using AutoMapper;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamiliesByIdsQueryHandler _handler;

    public GetFamiliesByIdsQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFamiliesByIdsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFamilies_WhenGivenValidIds()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3" };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        var query = new GetFamiliesByIdsQuery(new List<Guid> { family1.Id, family3.Id });

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
        result.Value.Should().HaveCount(2);
        result.Value!.Select(f => f.Id).Should().Contain(family1.Id);
        result.Value!.Select(f => f.Id).Should().Contain(family3.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenGivenNoIds()
    {
        // Arrange
        var query = new GetFamiliesByIdsQuery(new List<Guid>());

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
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenGivenNonExistentIds()
    {
        // Arrange
        var query = new GetFamiliesByIdsQuery(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

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
        result.Value.Should().BeEmpty();
    }
}
