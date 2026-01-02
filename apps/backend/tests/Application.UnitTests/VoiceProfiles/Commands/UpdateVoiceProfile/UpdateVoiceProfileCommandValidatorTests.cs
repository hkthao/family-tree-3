using backend.Application.VoiceProfiles.Commands.UpdateVoiceProfile;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.UpdateVoiceProfile;

public class UpdateVoiceProfileCommandValidatorTests
{
    private readonly UpdateVoiceProfileCommandValidator _validator;

    public UpdateVoiceProfileCommandValidatorTests()
    {
        _validator = new UpdateVoiceProfileCommandValidator();
    }

    [Fact]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.Empty,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID hồ sơ giọng nói không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLabelIsEmpty()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Label)
              .WithErrorMessage("Label không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLabelExceedsMaxLength()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = new string('a', 101),
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Label)
              .WithErrorMessage("Label không được vượt quá 100 ký tự.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenAudioUrlIsEmpty()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AudioUrl)
              .WithErrorMessage("Audio URL không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenAudioUrlIsInvalid()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "invalid-url",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AudioUrl)
              .WithErrorMessage("Audio URL không hợp lệ.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenDurationSecondsIsZeroOrLess()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 0,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
              .WithErrorMessage("Thời lượng audio phải lớn hơn 0.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLanguageIsEmpty()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Language)
              .WithErrorMessage("Ngôn ngữ không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLanguageExceedsMaxLength()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "verylonglanguagecode",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Language)
              .WithErrorMessage("Ngôn ngữ không được vượt quá 10 ký tự.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true,
            Status = Domain.Enums.VoiceProfileStatus.Active
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
