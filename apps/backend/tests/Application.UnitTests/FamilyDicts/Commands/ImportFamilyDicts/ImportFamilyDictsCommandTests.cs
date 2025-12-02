using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.FamilyDicts.Commands.ImportFamilyDicts;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyDicts.Commands.ImportFamilyDicts;

public class ImportFamilyDictsCommandTests : TestBase
{
    public ImportFamilyDictsCommandTests()
    {
        // Set up authenticated user by default for most tests
        _mockUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _mockUser.Setup(c => c.IsAuthenticated).Returns(true);
        // Default to admin for these command tests, as most successful tests require admin rights
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
    }

    [Fact]
    public async Task Handle_ShouldImportFamilyDicts()
    {
        // Arrange
        // Clear existing data to ensure test isolation
        _context.FamilyDicts.RemoveRange(_context.FamilyDicts);
        await _context.SaveChangesAsync();

        var handler = new ImportFamilyDictsCommandHandler(_context, _mapper, _mockAuthorizationService.Object);
        var command = new ImportFamilyDictsCommand
        {
            FamilyDicts = new List<FamilyDictImportDto>
            {
                new FamilyDictImportDto
                {
                    Name = "Imported FamilyDict 1",
                    Type = FamilyDictType.Blood,
                    Description = "Description 1",
                    Lineage = FamilyDictLineage.Noi,
                    SpecialRelation = false,
                    NamesByRegion = new NamesByRegionImportDto { North = "N1", Central = "C1", South = "S1" }
                },
                new FamilyDictImportDto
                {
                    Name = "Imported FamilyDict 2",
                    Type = FamilyDictType.Adoption,
                    Description = "Description 2",
                    Lineage = FamilyDictLineage.Ngoai,
                    SpecialRelation = true,
                    NamesByRegion = new NamesByRegionImportDto { North = "N2", Central = "C2", South = "S2" }
                }
            }
        };

        // Act
        var ids = await handler.Handle(command, CancellationToken.None);

        // Assert
        ids.Should().NotBeNull().And.HaveCount(2);
        _context.FamilyDicts.Should().HaveCount(2);

        var importedDict1 = _context.FamilyDicts.FirstOrDefault(f => f.Id == ids.First());
        importedDict1.Should().NotBeNull();
        importedDict1?.Name.Should().Be("Imported FamilyDict 1");
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Simulate non-admin user
        var handler = new ImportFamilyDictsCommandHandler(_context, _mapper, _mockAuthorizationService.Object);
        var command = new ImportFamilyDictsCommand
        {
            FamilyDicts = new List<FamilyDictImportDto>
            {
                new FamilyDictImportDto
                {
                    Name = "Imported FamilyDict",
                    Type = FamilyDictType.Blood,
                    Description = "Description",
                    Lineage = FamilyDictLineage.Noi,
                    SpecialRelation = false,
                    NamesByRegion = new NamesByRegionImportDto { North = "N", Central = "C", South = "S" }
                }
            }
        };

        // Act & Assert
        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Chỉ quản trị viên mới được phép nhập FamilyDict.");
    }
}
