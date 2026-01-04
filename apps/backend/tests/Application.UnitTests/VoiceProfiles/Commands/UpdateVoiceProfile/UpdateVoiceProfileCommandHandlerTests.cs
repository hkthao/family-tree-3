using backend.Application.Common.Exceptions;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.UpdateVoiceProfile;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.UpdateVoiceProfile;

public class UpdateVoiceProfileCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldUpdateVoiceProfileAndReturnSuccess()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new UpdateVoiceProfileCommandHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Old Label", "http://old.wav", 5.0, 0.0, "unknown", "{}", "en", false);
        context.VoiceProfiles.Add(voiceProfile);
        await context.SaveChangesAsync();

        var command = new UpdateVoiceProfileCommand
        {
            Id = voiceProfile.Id,
            Label = "New Label",
            AudioUrl = "http://new.wav",
            DurationSeconds = 12.3,
            Language = "fr",
            Consent = true,
            Status = VoiceProfileStatus.Archived
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var updatedVoiceProfile = await context.VoiceProfiles.FindAsync(voiceProfile.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(voiceProfile.Id);
        result.Value!.Label.Should().Be(command.Label);
        result.Value!.AudioUrl.Should().Be(command.AudioUrl);
        result.Value!.DurationSeconds.Should().Be(command.DurationSeconds);
        result.Value!.Language.Should().Be(command.Language);
        result.Value!.Consent.Should().Be(command.Consent);
        result.Value!.Status.Should().Be(command.Status);

        updatedVoiceProfile.Should().NotBeNull();
        updatedVoiceProfile!.Label.Should().Be(command.Label);
        updatedVoiceProfile.AudioUrl.Should().Be(command.AudioUrl);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVoiceProfileDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new UpdateVoiceProfileCommandHandler(context, _mapper);

        // Arrange
        var command = new UpdateVoiceProfileCommand
        {
            Id = Guid.NewGuid(),
            Label = "Non Existent",
            AudioUrl = "http://nonexistent.wav",
            DurationSeconds = 1.0,
            Language = "en",
            Consent = true,
            Status = VoiceProfileStatus.Active
        };

        // Act & Assert
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Voice Profile with ID {command.Id} not found.");
    }
}
