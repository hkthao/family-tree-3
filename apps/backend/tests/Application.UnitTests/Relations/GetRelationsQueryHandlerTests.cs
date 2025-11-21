using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Relations;
using backend.Application.Relations.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Relations;

public class GetRelationsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetRelationsQueryHandler _handler;

    public GetRelationsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetRelationsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllRelationsPaginated()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "relation1",
            Name = "Relation 1",
            Type = RelationType.Blood,
            Description = "Description 1",
            Lineage = RelationLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        var relation2 = new Relation
        {
            Id = "relation2",
            Name = "Relation 2",
            Type = RelationType.InLaw,
            Description = "Description 2",
            Lineage = RelationLineage.Ngoai,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R2", Central = "R2", South = "R2" }
        };
        var relation3 = new Relation
        {
            Id = "relation3",
            Name = "Relation 3",
            Type = RelationType.Adoption,
            Description = "Description 3",
            Lineage = RelationLineage.NoiNgoai,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R3", Central = "R3", South = "R3" }
        };

        _context.Relations.AddRange(relation1, relation2, relation3);
        await _context.SaveChangesAsync();

        var query = new GetRelationsQuery { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(3); // Changed from TotalCount to TotalItems
        result.Items.First().Id.Should().Be("relation1");
        result.Items.Last().Id.Should().Be("relation2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRelationsExist()
    {
        // Arrange
        var query = new GetRelationsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
