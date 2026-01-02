using backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.VoiceProfiles.Queries.GetVoiceProfileById;

public class GetVoiceProfileByIdQueryValidatorTests
{
    private readonly GetVoiceProfileByIdQueryValidator _validator;

    public GetVoiceProfileByIdQueryValidatorTests()
    {
        _validator = new GetVoiceProfileByIdQueryValidator();
    }

    [Fact]
    public async Task ShouldHaveErrorWhenIdIsEmpty()
    {
        var query = new GetVoiceProfileByIdQuery
        {
            Id = Guid.Empty
        };

        var result = await _validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("ID hồ sơ giọng nói không được để trống.");
    }

    [Fact]
    public async Task ShouldNotHaveAnyValidationErrorsWhenCommandIsValid()
    {
        var query = new GetVoiceProfileByIdQuery
        {
            Id = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
