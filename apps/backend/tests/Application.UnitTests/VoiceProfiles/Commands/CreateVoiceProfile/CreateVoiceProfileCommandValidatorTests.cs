using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;
using backend.Domain.Entities;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.CreateVoiceProfile;

public class CreateVoiceProfileCommandValidatorTests : TestBase
{
    public CreateVoiceProfileCommandValidatorTests()
    {
        // No class-level _validator initialization needed
    }

    [Fact]
    public async Task ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);

        var command = new CreateVoiceProfileCommand
        {
            MemberId = Guid.Empty,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member ID không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenMemberDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        // Member does not exist in context

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member không tồn tại.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLabelIsEmpty()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Label)
              .WithErrorMessage("Label không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLabelExceedsMaxLength()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = new string('a', 101),
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Label)
              .WithErrorMessage("Label không được vượt quá 100 ký tự.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenAudioUrlIsEmpty()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AudioUrl)
              .WithErrorMessage("Audio URL không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenAudioUrlIsInvalid()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "invalid-url",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AudioUrl)
              .WithErrorMessage("Audio URL không hợp lệ.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenDurationSecondsIsZeroOrLess()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 0,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
              .WithErrorMessage("Thời lượng audio phải lớn hơn 0.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLanguageIsEmpty()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Language)
              .WithErrorMessage("Ngôn ngữ không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenLanguageExceedsMaxLength()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "verylonglanguagecode",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Language)
              .WithErrorMessage("Ngôn ngữ không được vượt quá 10 ký tự.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenExceedingMaxActiveVoiceProfiles()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var validator = new CreateVoiceProfileCommandValidator(context);
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Profile1", "http://1.wav", 10, 0.0, "unknown", "{}", "vi", true));
        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Profile2", "http://2.wav", 10, 0.0, "unknown", "{}", "vi", true));
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Mỗi thành viên chỉ được có tối đa 2 hồ sơ giọng nói đang hoạt động.");
    }

    [Fact]
    public async Task ShouldNotHaveErrorWhenValidCommandAndWithinActiveVoiceProfileLimit()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid(); // Member needs a FamilyId
        var member = new Member("Last", "First", "CODE1", familyId) { Id = memberId }; // Explicitly set Id
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var validator = new CreateVoiceProfileCommandValidator(context);

        context.VoiceProfiles.Add(new VoiceProfile(memberId, "Profile1", "http://1.wav", 10, 0.0, "unknown", "{}", "vi", true));
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Test",
            AudioUrl = "http://test.com/audio.wav",
            DurationSeconds = 10,
            Language = "en",
            Consent = true
        };

        var result = await validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
