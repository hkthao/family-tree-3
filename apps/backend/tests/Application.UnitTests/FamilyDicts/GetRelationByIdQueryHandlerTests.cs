using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts;

public class GetFamilyDictByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamilyDictByIdQueryHandler _handler;

    public GetFamilyDictByIdQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetFamilyDictByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyDict_WhenIdExists()
    {
        // Arrange
        var familyDict = new FamilyDict
        {
            Id = "test_id",
            Name = "Test FamilyDict",
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "TR", Central = "TR", South = "TR" }
        };
        _context.FamilyDicts.Add(familyDict);
        await _context.SaveChangesAsync();

        var query = new GetFamilyDictByIdQuery("test_id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test_id");
        result.Name.Should().Be("Test FamilyDict");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var query = new GetFamilyDictByIdQuery("non_existent_id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
