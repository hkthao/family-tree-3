using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.FamilyDicts.Commands.UpdateFamilyDict;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts.Commands.UpdateFamilyDict;

public class UpdateFamilyDictCommandTests : TestBase
{
    private new readonly IMapper _mapper;

    public UpdateFamilyDictCommandTests() : base()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configurationProvider.CreateMapper();
    }

    [Fact]
    public async Task Handle_ShouldUpdateFamilyDict()
    {
        // Arrange
        var initialFamilyDict = new FamilyDict
        {
            Id = Guid.NewGuid(),
            Name = "Initial Name",
            Type = FamilyDictType.Blood,
            Description = "Initial Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegion { North = "Initial N", Central = "Initial C", South = "Initial S" }
        };
        _context.FamilyDicts.Add(initialFamilyDict);
        await _context.SaveChangesAsync();

        var handler = new UpdateFamilyDictCommandHandler(_context, _mapper);
        var command = new UpdateFamilyDictCommand
        {
            Id = initialFamilyDict.Id,
            Name = "Updated Name",
            Type = FamilyDictType.Adoption,
            Description = "Updated Description",
            Lineage = FamilyDictLineage.Ngoai,
            SpecialRelation = true,
            NamesByRegion = new NamesByRegionUpdateCommandDto { North = "Updated N", Central = "Updated C", South = "Updated S" }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedFamilyDict = await _context.FamilyDicts.FindAsync(initialFamilyDict.Id);

        updatedFamilyDict.Should().NotBeNull();
        updatedFamilyDict?.Name.Should().Be(command.Name);
        updatedFamilyDict?.Type.Should().Be(command.Type);
        updatedFamilyDict?.Description.Should().Be(command.Description);
        updatedFamilyDict?.Lineage.Should().Be(command.Lineage);
        updatedFamilyDict?.SpecialRelation.Should().Be(command.SpecialRelation);
        updatedFamilyDict?.NamesByRegion.Should().NotBeNull();
        updatedFamilyDict?.NamesByRegion.North.Should().Be(command.NamesByRegion.North);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDictDoesNotExist()
    {
        // Arrange
        var handler = new UpdateFamilyDictCommandHandler(_context, _mapper);
        var command = new UpdateFamilyDictCommand
        {
            Id = Guid.NewGuid(), // Non-existent ID
            Name = "Name",
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionUpdateCommandDto { North = "N", Central = "C", South = "S" }
        };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var handler = new UpdateFamilyDictCommandHandler(_context, _mapper);
        var command = new UpdateFamilyDictCommand
        {
            Id = Guid.NewGuid(),
            Name = "", // Empty name
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionUpdateCommandDto { North = "N", Central = "C", South = "S" }
        };

        var validator = new UpdateFamilyDictCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Act & Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Tên không được để trống.");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsTooLong()
    {
        // Arrange
        var handler = new UpdateFamilyDictCommandHandler(_context, _mapper);
        var command = new UpdateFamilyDictCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201), // Too long name
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionUpdateCommandDto { North = "N", Central = "C", South = "S" }
        };

        var validator = new UpdateFamilyDictCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Act & Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Tên không được vượt quá 200 ký tự.");
    }
}
