using backend.Application.Common.Exceptions;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Commands.ArchiveVoiceProfile;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Commands.ArchiveVoiceProfile;

public class ArchiveVoiceProfileCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldArchiveVoiceProfileAndReturnSuccess()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new ArchiveVoiceProfileCommandHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Test Label", "http://test.wav", 10.0, "en", true);
        context.VoiceProfiles.Add(voiceProfile);
        await context.SaveChangesAsync();

        var command = new ArchiveVoiceProfileCommand
        {
            Id = voiceProfile.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var archivedVoiceProfile = await context.VoiceProfiles.FindAsync(voiceProfile.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(voiceProfile.Id);
        result.Value!.Status.Should().Be(VoiceProfileStatus.Archived);

        archivedVoiceProfile.Should().NotBeNull();
        archivedVoiceProfile!.Status.Should().Be(VoiceProfileStatus.Archived);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVoiceProfileDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new ArchiveVoiceProfileCommandHandler(context, _mapper);

        // Arrange
        var command = new ArchiveVoiceProfileCommand
        {
            Id = Guid.NewGuid()
        };

        // Act & Assert
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Voice Profile with ID {command.Id} not found.");
    }
}
