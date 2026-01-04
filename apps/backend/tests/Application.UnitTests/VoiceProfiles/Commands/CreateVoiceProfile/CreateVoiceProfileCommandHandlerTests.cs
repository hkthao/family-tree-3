using backend.Application.Common.Constants;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq; // Added to enable mocking
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.CreateVoiceProfile;

public class CreateVoiceProfileCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateVoiceProfileAndReturnSuccess()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new CreateVoiceProfileCommandHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        await context.SaveChangesAsync();

        var command = new CreateVoiceProfileCommand
        {
            MemberId = memberId,
            Label = "Default Voice",
            AudioUrl = "http://example.com/audio.wav",
            DurationSeconds = 10.5,
            QualityScore = 80.0,
            OverallQuality = "pass",
            QualityMessages = "{\"messages\":[]}",
            Language = "vi",
            Consent = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var createdVoiceProfile = await context.VoiceProfiles.FindAsync(result.Value!.Id);

        result.Value.Id.Should().NotBeEmpty();
        result.Value.MemberId.Should().Be(memberId);
        result.Value.Label.Should().Be(command.Label);
        result.Value.AudioUrl.Should().Be(command.AudioUrl);
        result.Value.DurationSeconds.Should().Be(command.DurationSeconds);
        result.Value.Language.Should().Be(command.Language);
        result.Value.Consent.Should().Be(command.Consent);
        result.Value.Status.Should().Be(VoiceProfileStatus.Active);

        createdVoiceProfile.Should().NotBeNull();
        createdVoiceProfile!.MemberId.Should().Be(memberId);
    }
}
