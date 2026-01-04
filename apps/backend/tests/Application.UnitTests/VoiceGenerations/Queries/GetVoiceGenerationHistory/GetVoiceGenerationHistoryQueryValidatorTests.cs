using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Queries.GetVoiceGenerationHistory;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.VoiceGenerations.Queries.GetVoiceGenerationHistory;

public class GetVoiceGenerationHistoryQueryValidatorTests : TestBase
{
    public GetVoiceGenerationHistoryQueryValidatorTests()
    {
        // No class-level _validator initialization needed
    }

    [Fact]
    public async Task ShouldHaveErrorWhenVoiceProfileIdIsEmpty()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GetVoiceGenerationHistoryQueryValidator(context);
        var query = new GetVoiceGenerationHistoryQuery
        {
            VoiceProfileId = Guid.Empty
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.VoiceProfileId)
              .WithErrorMessage("Voice Profile ID không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenVoiceProfileDoesNotExist()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GetVoiceGenerationHistoryQueryValidator(context);
        var voiceProfileId = Guid.NewGuid();
        // VoiceProfile does not exist in context

        var query = new GetVoiceGenerationHistoryQuery
        {
            VoiceProfileId = voiceProfileId
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.VoiceProfileId)
              .WithErrorMessage("Voice Profile không tồn tại.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GetVoiceGenerationHistoryQueryValidator(context);
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Label", "http://audio.wav", 10, 0.0, "unknown", "{}", "en", true));
        await context.SaveChangesAsync();

        var query = new GetVoiceGenerationHistoryQuery
        {
            VoiceProfileId = context.VoiceProfiles.First().Id
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
