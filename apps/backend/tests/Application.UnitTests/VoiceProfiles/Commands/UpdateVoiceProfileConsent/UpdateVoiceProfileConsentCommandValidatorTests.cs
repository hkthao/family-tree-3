using backend.Application.VoiceProfiles.Commands.UpdateVoiceProfileConsent;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.UpdateVoiceProfileConsent;

public class UpdateVoiceProfileConsentCommandValidatorTests
{
    private readonly UpdateVoiceProfileConsentCommandValidator _validator;

    public UpdateVoiceProfileConsentCommandValidatorTests()
    {
        _validator = new UpdateVoiceProfileConsentCommandValidator();
    }

    [Fact]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new UpdateVoiceProfileConsentCommand
        {
            Id = Guid.Empty,
            Consent = true
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID hồ sơ giọng nói không được để trống.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var command = new UpdateVoiceProfileConsentCommand
        {
            Id = Guid.NewGuid(),
            Consent = true
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
