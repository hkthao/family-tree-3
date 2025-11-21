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

public class GetFamilyDictsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamilyDictsQueryHandler _handler;

    public GetFamilyDictsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetFamilyDictsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFamilyDictsPaginated()
    {
        // Arrange
        var familyDict1 = new FamilyDict
        {
            Id = "familyDict1",
            Name = "FamilyDict 1",
            Type = FamilyDictType.Blood,
            Description = "Description 1",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        var familyDict2 = new FamilyDict
        {
            Id = "familyDict2",
            Name = "FamilyDict 2",
            Type = FamilyDictType.InLaw,
            Description = "Description 2",
            Lineage = FamilyDictLineage.Ngoai,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R2", Central = "R2", South = "R2" }
        };
        var familyDict3 = new FamilyDict
        {
            Id = "familyDict3",
            Name = "FamilyDict 3",
            Type = FamilyDictType.Adoption,
            Description = "Description 3",
            Lineage = FamilyDictLineage.NoiNgoai,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R3", Central = "R3", South = "R3" }
        };

        _context.FamilyDicts.AddRange(familyDict1, familyDict2, familyDict3);
        await _context.SaveChangesAsync();

        var query = new GetFamilyDictsQuery { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(3); // Changed from TotalCount to TotalItems
        result.Items.First().Id.Should().Be("familyDict1");
        result.Items.Last().Id.Should().Be("familyDict2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFamilyDictsExist()
    {
        // Arrange
        var query = new GetFamilyDictsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
