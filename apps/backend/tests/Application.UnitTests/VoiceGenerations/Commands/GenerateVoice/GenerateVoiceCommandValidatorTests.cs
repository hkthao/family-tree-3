using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceGenerations.Commands.GenerateVoice;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.VoiceGenerations.Commands.GenerateVoice;

public class GenerateVoiceCommandValidatorTests : TestBase
{
    public GenerateVoiceCommandValidatorTests()
    {
        // No class-level _validator initialization needed
    }

    [Fact]
    public async Task ShouldHaveErrorWhenVoiceProfileIdIsEmpty()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GenerateVoiceCommandValidator(context);
        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = Guid.Empty,
            Text = "Test text."
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.VoiceProfileId)
              .WithErrorMessage("Voice Profile ID không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenVoiceProfileDoesNotExist()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GenerateVoiceCommandValidator(context);
        var voiceProfileId = Guid.NewGuid();
        // VoiceProfile does not exist in context

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = voiceProfileId,
            Text = "Test text."
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.VoiceProfileId)
              .WithErrorMessage("Voice Profile không tồn tại.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenTextIsEmpty()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GenerateVoiceCommandValidator(context);
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Label", "http://audio.wav", 10, "en", true));
        await context.SaveChangesAsync();

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = context.VoiceProfiles.First().Id,
            Text = ""
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Text)
              .WithErrorMessage("Văn bản không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenTextExceedsMaxLength()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GenerateVoiceCommandValidator(context);
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Label", "http://audio.wav", 10, "en", true));
        await context.SaveChangesAsync();

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = context.VoiceProfiles.First().Id,
            Text = new string('a', 4001)
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Text)
              .WithErrorMessage("Văn bản không được vượt quá 4000 ký tự.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GenerateVoiceCommandValidator(context);
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Label", "http://audio.wav", 10, "en", true));
        await context.SaveChangesAsync();

        var command = new GenerateVoiceCommand
        {
            VoiceProfileId = context.VoiceProfiles.First().Id,
            Text = "Valid test text."
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
