using backend.Application.Common.Exceptions;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Queries.GetVoiceProfileById;

public class GetVoiceProfileByIdQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnVoiceProfile_WhenVoiceProfileExists()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceProfileByIdQueryHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Test Label", "http://test.wav", 10.0, 0.0, "unknown", "{}", "en", true);
        context.VoiceProfiles.Add(voiceProfile);
        await context.SaveChangesAsync();

        var query = new GetVoiceProfileByIdQuery { Id = voiceProfile.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(voiceProfile.Id);
        result.Value!.Label.Should().Be(voiceProfile.Label);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVoiceProfileDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceProfileByIdQueryHandler(context, _mapper);

        // Arrange
        var query = new GetVoiceProfileByIdQuery { Id = Guid.NewGuid() };

        // Act & Assert
        await FluentActions.Awaiting(() => handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Entity \"VoiceProfile\" ({query.Id}) was not found.");
    }
}
