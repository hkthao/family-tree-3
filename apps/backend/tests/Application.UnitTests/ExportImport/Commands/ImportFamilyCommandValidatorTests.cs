using backend.Application.ExportImport.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.ExportImport.Commands;

public class ImportFamilyCommandValidatorTests
{
    private readonly ImportFamilyCommandValidator _validator;

    public ImportFamilyCommandValidatorTests()
    {
        _validator = new ImportFamilyCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyDataIsNull()
    {
        var command = new ImportFamilyCommand { FamilyData = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FamilyData)
              .WithErrorMessage("Family data cannot be null.");
    }

    // You might want to add more granular tests for FamilyExportDto properties if needed
    // However, for this command validator, ensuring FamilyData is not null might be sufficient
    // as detailed validation of FamilyExportDto structure would typically be in a separate validator.

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new ImportFamilyCommand
        {
            FamilyData = new FamilyExportDto
            {
                Name = "Test Family",
                Code = "TF1",
                Visibility = "Private",
                Members = new List<MemberExportDto>(),
                Relationships = new List<RelationshipExportDto>(),
                Events = new List<EventExportDto>()
            },
            ClearExistingData = true
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
