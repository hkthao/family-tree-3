using backend.Application.FamilyDicts.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts.Queries.GetSpecialFamilyDicts;

public class GetSpecialFamilyDictsQueryHandlerTests : TestBase
{
    private readonly GetSpecialFamilyDictsQueryHandler _handler;

    public GetSpecialFamilyDictsQueryHandlerTests() : base()
    {
        _handler = new GetSpecialFamilyDictsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecialFamilyDictsPaginated()
    {
        // Arrange
        var familyDict1 = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "Ông nội",
            Type = FamilyDictType.Blood,
            Description = "Cha của cha bạn",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        var familyDict2 = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "Cha nuôi",
            Type = FamilyDictType.Adoption,
            Description = "Người nhận con làm cha nuôi",
            Lineage = FamilyDictLineage.NoiNgoai,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R2", Central = "R2", South = "R2" }
        };
        var familyDict3 = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "Mẹ kế",
            Type = FamilyDictType.InLaw,
            Description = "Vợ của cha nhưng không phải mẹ ruột",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegion { North = "R3", Central = "R3", South = "R3" }
        };

        _context.FamilyDicts.AddRange(familyDict1, familyDict2, familyDict3);
        await _context.SaveChangesAsync();

        var query = new GetSpecialFamilyDictsQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2); // Changed from TotalCount to TotalItems
        result.Items.Should().Contain(r => r.Id == familyDict2.Id);
        result.Items.Should().Contain(r => r.Id == familyDict3.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSpecialFamilyDictsExist()
    {
        // Arrange
        var familyDict1 = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "Ông nội",
            Type = FamilyDictType.Blood,
            Description = "Cha của cha bạn",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "R1", Central = "R1", South = "R1" }
        };
        _context.FamilyDicts.Add(familyDict1);
        await _context.SaveChangesAsync();

        var query = new GetSpecialFamilyDictsQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0); // Changed from TotalCount to TotalItems
    }
}
