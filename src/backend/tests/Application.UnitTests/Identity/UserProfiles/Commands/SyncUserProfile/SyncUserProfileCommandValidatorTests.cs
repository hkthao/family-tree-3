using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using System.Security.Claims;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncUserProfile;

public class SyncUserProfileCommandValidatorTests
{
    private readonly SyncUserProfileCommandValidator _validator;

    public SyncUserProfileCommandValidatorTests()
    {
        _validator = new SyncUserProfileCommandValidator();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        var command = new SyncUserProfileCommand { UserPrincipal = new ClaimsPrincipal() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenUserPrincipalIsNull()
    {
        var command = new SyncUserProfileCommand { UserPrincipal = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserPrincipal);
    }
}
