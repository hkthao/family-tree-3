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

public class SearchFamilyDictsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly SearchFamilyDictsQueryHandler _handler;

    public SearchFamilyDictsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new SearchFamilyDictsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilyDicts_WhenSearchTermIsProvided()
    {
        // Arrange
        var familyDict1 = new FamilyDict
        {
            Id = "r1",
            Name = "Ông nội",
            Type = FamilyDictType.Blood,
            Description = "Cha của cha bạn",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        var familyDict2 = new FamilyDict
        {
            Id = "r2",
            Name = "Bà ngoại",
            Type = FamilyDictType.Blood,
            Description = "Mẹ của mẹ bạn",
            Lineage = FamilyDictLineage.Ngoai,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Bà ngoại", Central = "Bà ngoại", South = "Bà ngoại" }
        };
        _context.FamilyDicts.AddRange(familyDict1, familyDict2);
        await _context.SaveChangesAsync();

        var query = new SearchFamilyDictsQuery { Q = "Ông" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Ông nội");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilyDicts_WhenLineageIsProvided()
    {
        // Arrange
        var familyDict1 = new FamilyDict
        {
            Id = "r1",
            Name = "Ông nội",
            Type = FamilyDictType.Blood,
            Description = "Cha của cha bạn",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        var familyDict2 = new FamilyDict
        {
            Id = "r2",
            Name = "Bà ngoại",
            Type = FamilyDictType.Blood,
            Description = "Mẹ của mẹ bạn",
            Lineage = FamilyDictLineage.Ngoai,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Bà ngoại", Central = "Bà ngoại", South = "Bà ngoại" }
        }; _context.FamilyDicts.AddRange(familyDict1, familyDict2);
        await _context.SaveChangesAsync();

        var query = new SearchFamilyDictsQuery { Lineage = FamilyDictLineage.Noi };

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
        var familyDict1 = new FamilyDict
        {
            Id = "r1",
            Name = "Ông nội",
            Type = FamilyDictType.Blood,
            Description = "Cha của cha bạn",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Ông nội", Central = "Ông nội", South = "Ông nội" }
        };
        _context.FamilyDicts.Add(familyDict1);
        await _context.SaveChangesAsync();

        var query = new SearchFamilyDictsQuery { Q = "Không tồn tại" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
