using backend.Application.FamilyDicts.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts;

public class GetFamilyDictByIdQueryHandlerTests : TestBase
{
    private readonly GetFamilyDictByIdQueryHandler _handler;

    public GetFamilyDictByIdQueryHandlerTests() : base()
    {
        _handler = new GetFamilyDictByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyDict_WhenIdExists()
    {
        // Arrange
        var familyDictId = Guid.NewGuid();
        var familyDict = new FamilyDict
        {
            Id = familyDictId,
            Name = "Test FamilyDict",
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "TR", Central = "TR", South = "TR" }
        };
        _context.FamilyDicts.Add(familyDict);
        await _context.SaveChangesAsync();

        var query = new GetFamilyDictByIdQuery(familyDictId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(familyDictId);
        result.Name.Should().Be("Test FamilyDict");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var query = new GetFamilyDictByIdQuery(Guid.NewGuid()); // Non-existent ID

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
