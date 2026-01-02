using backend.Application.VoiceProfiles.Commands.ArchiveVoiceProfile;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.ArchiveVoiceProfile;

public class ArchiveVoiceProfileCommandValidatorTests
{
    private readonly ArchiveVoiceProfileCommandValidator _validator;

    public ArchiveVoiceProfileCommandValidatorTests()
    {
        _validator = new ArchiveVoiceProfileCommandValidator();
    }

    [Fact]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new ArchiveVoiceProfileCommand
        {
            Id = Guid.Empty
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID hồ sơ giọng nói không được để trống.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var command = new ArchiveVoiceProfileCommand
        {
            Id = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
