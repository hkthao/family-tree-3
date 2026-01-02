using backend.Application.Common.Exceptions;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.UpdateVoiceProfileConsent;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.UpdateVoiceProfileConsent;

public class UpdateVoiceProfileConsentCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldUpdateVoiceProfileConsentAndReturnSuccess()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new UpdateVoiceProfileConsentCommandHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Test Label", "http://test.wav", 10.0, "en", false);
        context.VoiceProfiles.Add(voiceProfile);
        await context.SaveChangesAsync();

        var command = new UpdateVoiceProfileConsentCommand
        {
            Id = voiceProfile.Id,
            Consent = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var updatedVoiceProfile = await context.VoiceProfiles.FindAsync(voiceProfile.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(voiceProfile.Id);
        result.Value!.Consent.Should().Be(command.Consent);

        updatedVoiceProfile.Should().NotBeNull();
        updatedVoiceProfile!.Consent.Should().Be(command.Consent);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVoiceProfileDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new UpdateVoiceProfileConsentCommandHandler(context, _mapper);

        // Arrange
        var command = new UpdateVoiceProfileConsentCommand
        {
            Id = Guid.NewGuid(),
            Consent = true
        };

        // Act & Assert
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Voice Profile with ID {command.Id} not found.");
    }
}
