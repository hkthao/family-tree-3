using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.FamilyDicts.Commands.CreateFamilyDict;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Exceptions; // Added for ForbiddenAccessException

namespace backend.Application.UnitTests.FamilyDicts.Commands.CreateFamilyDict;

public class CreateFamilyDictCommandTests : TestBase
{
    public CreateFamilyDictCommandTests()
    {
        // Set up authenticated user by default for most tests
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        // Default to admin for these command tests, as most successful tests require admin rights
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
    }

    [Fact]
    public async Task Handle_ShouldPersistFamilyDict()
    {
        // Arrange
        var handler = new CreateFamilyDictCommandHandler(_context, _mockUser.Object, _mapper, _mockAuthorizationService.Object);
        var command = new CreateFamilyDictCommand
        {
            Name = "New FamilyDict",
            Type = FamilyDictType.Blood,
            Description = "Description for new FamilyDict",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionCommandDto { North = "New N", Central = "New C", South = "New S" }
        };

        // Act
        var id = await handler.Handle(command, CancellationToken.None);

        // Assert
        var familyDict = await _context.FamilyDicts.FindAsync(id);

        familyDict.Should().NotBeNull();
        familyDict?.Name.Should().Be(command.Name);
        familyDict?.Type.Should().Be(command.Type);
        familyDict?.Description.Should().Be(command.Description);
        familyDict?.Lineage.Should().Be(command.Lineage);
        familyDict?.SpecialRelation.Should().Be(command.SpecialRelation);
        familyDict?.NamesByRegion.Should().NotBeNull();
        familyDict?.NamesByRegion.North.Should().Be(command.NamesByRegion.North);
        // CreatedBy is set by AuditableEntityService, not directly by handler, so no assertion here
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange
        // Note: For validation tests, authorization check is not the primary focus,
        // but the handler still needs the authorizationService mock.
        var handler = new CreateFamilyDictCommandHandler(_context, _mockUser.Object, _mapper, _mockAuthorizationService.Object);
        var command = new CreateFamilyDictCommand
        {
            Name = "", // Empty name
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionCommandDto { North = "N", Central = "C", South = "S" }
        };

        var validator = new CreateFamilyDictCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Act & Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Tên không được để trống.");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsTooLong()
    {
        // Arrange
        var handler = new CreateFamilyDictCommandHandler(_context, _mockUser.Object, _mapper, _mockAuthorizationService.Object);
        var command = new CreateFamilyDictCommand
        {
            Name = new string('A', 201), // Too long name
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionCommandDto { North = "N", Central = "C", South = "S" }
        };

        var validator = new CreateFamilyDictCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Act & Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Tên không được vượt quá 200 ký tự.");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNorthNamesByRegionIsEmpty()
    {
        // Arrange
        var handler = new CreateFamilyDictCommandHandler(_context, _mockUser.Object, _mapper, _mockAuthorizationService.Object);
        var command = new CreateFamilyDictCommand
        {
            Name = "Valid Name",
            Type = FamilyDictType.Blood,
            Description = "Description",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionCommandDto { North = "", Central = "C", South = "S" } // Empty North
        };

        var validator = new CreateFamilyDictCommandValidator();
        var validationResult = await validator.ValidateAsync(command);

        // Act & Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Tên miền Bắc không được để trống.");
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Simulate non-admin user
        var handler = new CreateFamilyDictCommandHandler(_context, _mockUser.Object, _mapper, _mockAuthorizationService.Object);
        var command = new CreateFamilyDictCommand
        {
            Name = "New FamilyDict",
            Type = FamilyDictType.Blood,
            Description = "Description for new FamilyDict",
            Lineage = FamilyDictLineage.Noi,
            SpecialRelation = false,
            NamesByRegion = new NamesByRegionCommandDto { North = "New N", Central = "New C", South = "New S" }
        };

        // Act & Assert
        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Chỉ quản trị viên mới được phép tạo FamilyDict.");
    }
}