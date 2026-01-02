using backend.Application.VoiceProfiles.Commands.ActivateVoiceProfile;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.ActivateVoiceProfile;

public class ActivateVoiceProfileCommandValidatorTests
{
    private readonly ActivateVoiceProfileCommandValidator _validator;

    public ActivateVoiceProfileCommandValidatorTests()
    {
        _validator = new ActivateVoiceProfileCommandValidator();
    }

    [Fact]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new ActivateVoiceProfileCommand
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
        var command = new ActivateVoiceProfileCommand
        {
            Id = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
