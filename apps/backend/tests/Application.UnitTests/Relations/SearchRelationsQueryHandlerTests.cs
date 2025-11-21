using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Relations;
using backend.Application.Relations.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Mappings;

namespace backend.Application.UnitTests.Relations;

public class SearchRelationsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly SearchRelationsQueryHandler _handler;

    public SearchRelationsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new SearchRelationsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredRelations_WhenSearchTermIsProvided()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "r1", Name = "Ông nội", Type = RelationType.Blood, Description = "Cha của cha bạn",
            Lineage = RelationLineage.Noi, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        var relation2 = new Relation
        {
            Id = "r2", Name = "Bà ngoại", Type = RelationType.Blood, Description = "Mẹ của mẹ bạn",
            Lineage = RelationLineage.Ngoai, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Bà ngoại", Central = "Bà ngoại", South = "Bà ngoại" }
        };
        _context.Relations.AddRange(relation1, relation2);
        await _context.SaveChangesAsync();

        var query = new SearchRelationsQuery { Q = "Ông" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Ông nội");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredRelations_WhenLineageIsProvided()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "r1", Name = "Ông nội", Type = RelationType.Blood, Description = "Cha của cha bạn",
            Lineage = RelationLineage.Noi, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        var relation2 = new Relation
        {
            Id = "r2", Name = "Bà ngoại", Type = RelationType.Blood, Description = "Mẹ của mẹ bạn",
            Lineage = RelationLineage.Ngoai, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Bà ngoại", Central = "Bà ngoại", South = "Bà ngoại" }
        };
        _context.Relations.AddRange(relation1, relation2);
        await _context.SaveChangesAsync();

        var query = new SearchRelationsQuery { Lineage = RelationLineage.Noi };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Ông nội");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatchingRelations()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "r1", Name = "Ông nội", Type = RelationType.Blood, Description = "Cha của cha bạn",
            Lineage = RelationLineage.Noi, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        _context.Relations.Add(relation1);
        await _context.SaveChangesAsync();

        var query = new SearchRelationsQuery { Q = "Không tồn tại" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
