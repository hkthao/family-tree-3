using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Queries.GetVoiceGenerationHistory;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceGenerations.Queries.GetVoiceGenerationHistory;

public class GetVoiceGenerationHistoryQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnVoiceGenerationHistoryForGivenVoiceProfile()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceGenerationHistoryQueryHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile1 = new VoiceProfile(memberId, "Profile1", "http://1.wav", 5.0, 0.0, "unknown", "{}", "en", true);
        var voiceProfile2 = new VoiceProfile(memberId, "Profile2", "http://2.wav", 7.0, 0.0, "unknown", "{}", "en", true);
        context.VoiceProfiles.Add(voiceProfile1);
        context.VoiceProfiles.Add(voiceProfile2);

        context.VoiceGenerations.Add(new VoiceGeneration(voiceProfile1.Id, "Text1", "http://gen1.wav", 2.0));
        context.VoiceGenerations.Add(new VoiceGeneration(voiceProfile1.Id, "Text2", "http://gen2.wav", 3.0));
        context.VoiceGenerations.Add(new VoiceGeneration(voiceProfile2.Id, "Text3", "http://gen3.wav", 4.0));
        await context.SaveChangesAsync();

        var query = new GetVoiceGenerationHistoryQuery { VoiceProfileId = voiceProfile1.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.Should().AllSatisfy(vg => vg.VoiceProfileId.Should().Be(voiceProfile1.Id));
        result.Value!.Should().Contain(vg => vg.Text == "Text1");
        result.Value!.Should().Contain(vg => vg.Text == "Text2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoVoiceGenerationsForGivenVoiceProfile()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceGenerationHistoryQueryHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        var voiceProfile = new VoiceProfile(memberId, "Profile1", "http://1.wav", 5.0, 0.0, "unknown", "{}", "en", true);
        context.VoiceProfiles.Add(voiceProfile);
        await context.SaveChangesAsync();

        var query = new GetVoiceGenerationHistoryQuery { VoiceProfileId = voiceProfile.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenVoiceProfileDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceGenerationHistoryQueryHandler(context, _mapper);

        // Arrange
        var nonExistentVoiceProfileId = Guid.NewGuid();
        var query = new GetVoiceGenerationHistoryQuery { VoiceProfileId = nonExistentVoiceProfileId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Handler returns success with empty list if voice profile exists but no generations
        result.Value!.Should().BeEmpty();
    }
}
