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

public class GetSpecialFamilyDictsQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetSpecialFamilyDictsQueryHandler _handler;

    public GetSpecialFamilyDictsQueryHandlerTests() : base()
    {
        _mapperMock = new Mock<IMapper>();
        // Setup AutoMapper for testing
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // Assuming MappingProfile contains Relation mappings
        }));

        _handler = new GetSpecialFamilyDictsQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecialFamilyDictsPaginated()
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
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        var familyDict2 = new FamilyDict
        {
            Id = "r2",
            Name = "Cha nuôi",
            Type = FamilyDictType.Adoption,
            Description = "Người nhận con làm cha nuôi",
            Lineage = FamilyDictLineage.NoiNgoai,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R2", Central = "R2", South = "R2" }
        };
        var familyDict3 = new FamilyDict
        {
            Id = "r3",
            Name = "Mẹ kế",
            Type = FamilyDictType.InLaw,
            Description = "Vợ của cha nhưng không phải mẹ ruột",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R3", Central = "R3", South = "R3" }
        };

        _context.FamilyDicts.AddRange(familyDict1, familyDict2, familyDict3);
        await _context.SaveChangesAsync();

        var query = new GetSpecialFamilyDictsQuery { PageNumber = 1, PageSize = 10 };

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
    public async Task Handle_ShouldReturnEmptyList_WhenNoSpecialFamilyDictsExist()
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
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        _context.FamilyDicts.Add(familyDict1);
        await _context.SaveChangesAsync();

        var query = new GetSpecialFamilyDictsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
