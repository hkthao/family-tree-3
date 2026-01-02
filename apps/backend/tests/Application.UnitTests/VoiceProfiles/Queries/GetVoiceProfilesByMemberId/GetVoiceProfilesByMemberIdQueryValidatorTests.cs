using backend.Application.Common.Interfaces;
using backend.Application.UnitTests.Common;
using backend.Application.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;
using backend.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Queries.GetVoiceProfilesByMemberId;

public class GetVoiceProfilesByMemberIdQueryValidatorTests : TestBase
{
    public GetVoiceProfilesByMemberIdQueryValidatorTests()
    {
        // No class-level _validator initialization needed
    }

    [Fact]
    public async Task ShouldHaveErrorWhenMemberIdIsEmpty()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GetVoiceProfilesByMemberIdQueryValidator(context);
        var query = new GetVoiceProfilesByMemberIdQuery
        {
            MemberId = Guid.Empty
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member ID không được để trống.");
    }

    [Fact]
    public async Task ShouldHaveErrorWhenMemberDoesNotExist()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var validator = new GetVoiceProfilesByMemberIdQueryValidator(context);
        var memberId = Guid.NewGuid();
        // Member does not exist in context

        var query = new GetVoiceProfilesByMemberIdQuery
        {
            MemberId = memberId
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("Member không tồn tại.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var context = GetApplicationDbContext(); // Get fresh context
        var memberId = Guid.NewGuid();
        context.Members.Add(new Member("Last", "First", "CODE1", memberId));
        await context.SaveChangesAsync();

        var validator = new GetVoiceProfilesByMemberIdQueryValidator(context);

        var query = new GetVoiceProfilesByMemberIdQuery
        {
            MemberId = memberId
        };

        var result = await validator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
