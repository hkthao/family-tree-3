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

public class GetRelationByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetRelationByIdQueryHandler _handler;

    public GetRelationByIdQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetRelationByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRelation_WhenIdExists()
    {
        // Arrange
        var relation = new Relation
        {
            Id = "test_id",
            Name = "Test Relation",
            Type = RelationType.Blood,
            Description = "Description",
            Lineage = RelationLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "TR", Central = "TR", South = "TR" }
        };
        _context.Relations.Add(relation);
        await _context.SaveChangesAsync();

        var query = new GetRelationByIdQuery("test_id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test_id");
        result.Name.Should().Be("Test Relation");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var query = new GetRelationByIdQuery("non_existent_id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
