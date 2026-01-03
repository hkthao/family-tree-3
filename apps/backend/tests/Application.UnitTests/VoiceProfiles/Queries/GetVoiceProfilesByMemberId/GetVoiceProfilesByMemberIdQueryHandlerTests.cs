using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;

public class GetVoiceProfilesByMemberIdQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnVoiceProfilesForGivenMember()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceProfilesByMemberIdQueryHandler(context, _mapper);

        // Arrange
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();
        context.Members.Add(new Member("Last1", "First1", "CODE1", Guid.NewGuid()) { Id = memberId1 });
        context.Members.Add(new Member("Last2", "First2", "CODE2", Guid.NewGuid()) { Id = memberId2 });

        context.VoiceProfiles.Add(new VoiceProfile(memberId1, "Profile1", "http://1.wav", 5.0, "en", true));
        context.VoiceProfiles.Add(new VoiceProfile(memberId1, "Profile2", "http://2.wav", 7.0, "en", true));
        context.VoiceProfiles.Add(new VoiceProfile(memberId2, "Profile3", "http://3.wav", 10.0, "fr", true));
        await context.SaveChangesAsync();

        // Verify that the member exists in the context
        (await context.Members.AnyAsync(m => m.Id == memberId1, CancellationToken.None)).Should().BeTrue("Member with memberId1 should exist in the context.");

        var query = new GetVoiceProfilesByMemberIdQuery { MemberId = memberId1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(vp => vp.MemberId.Should().Be(memberId1));
        result.Value.Should().Contain(vp => vp.Label == "Profile1");
        result.Value.Should().Contain(vp => vp.Label == "Profile2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoVoiceProfilesForGivenMember()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceProfilesByMemberIdQueryHandler(context, _mapper);

        // Arrange
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", Guid.NewGuid()) { Id = memberId });
        await context.SaveChangesAsync();

        var query = new GetVoiceProfilesByMemberIdQuery { MemberId = memberId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenMemberDoesNotExist()
    {
        using var context = GetApplicationDbContext(); // Get fresh context
        var handler = new GetVoiceProfilesByMemberIdQueryHandler(context, _mapper);

        // Arrange
        var nonExistentMemberId = Guid.NewGuid();
        var query = new GetVoiceProfilesByMemberIdQuery { MemberId = nonExistentMemberId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be("Validation");
        result.ValidationErrors!["MemberId"].Should().Contain("Member không tồn tại.");
    }
}
