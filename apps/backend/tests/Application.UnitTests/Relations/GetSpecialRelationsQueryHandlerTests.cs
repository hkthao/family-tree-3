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

public class GetSpecialRelationsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetSpecialRelationsQueryHandler _handler;

    public GetSpecialRelationsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetSpecialRelationsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecialRelationsPaginated()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "r1", Name = "Ông nội", Type = RelationType.Blood, Description = "Cha của cha bạn",
            Lineage = RelationLineage.Noi, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        var relation2 = new Relation
        {
            Id = "r2", Name = "Cha nuôi", Type = RelationType.Adoption, Description = "Người nhận con làm cha nuôi",
            Lineage = RelationLineage.NoiNgoai, SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R2", Central = "R2", South = "R2" }
        };
        var relation3 = new Relation
        {
            Id = "r3", Name = "Mẹ kế", Type = RelationType.InLaw, Description = "Vợ của cha nhưng không phải mẹ ruột",
            Lineage = RelationLineage.Noi, SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R3", Central = "R3", South = "R3" }
        };

        _context.Relations.AddRange(relation1, relation2, relation3);
        await _context.SaveChangesAsync();

        var query = new GetSpecialRelationsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2); // Changed from TotalCount to TotalItems
        result.Items.Should().Contain(r => r.Id == "r2");
        result.Items.Should().Contain(r => r.Id == "r3");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSpecialRelationsExist()
    {
        // Arrange
        var relation1 = new Relation
        {
            Id = "r1", Name = "Ông nội", Type = RelationType.Blood, Description = "Cha của cha bạn",
            Lineage = RelationLineage.Noi, SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        _context.Relations.Add(relation1);
        await _context.SaveChangesAsync();

        var query = new GetSpecialRelationsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
