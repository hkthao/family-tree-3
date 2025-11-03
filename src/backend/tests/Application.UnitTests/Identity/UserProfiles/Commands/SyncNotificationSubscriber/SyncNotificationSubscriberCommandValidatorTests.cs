using backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;
using backend.Application.Identity.UserProfiles.Queries;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Commands.SyncNotificationSubscriber;

public class SyncNotificationSubscriberCommandValidatorTests
{
    private readonly SyncNotificationSubscriberCommandValidator _validator;

    public SyncNotificationSubscriberCommandValidatorTests()
    {
        _validator = new SyncNotificationSubscriberCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenUserProfileIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi UserProfile là null.
        var command = new SyncNotificationSubscriberCommand { UserProfile = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserProfile)
              .WithErrorMessage("User profile cannot be null.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenUserProfileIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi UserProfile hợp lệ.
        var command = new SyncNotificationSubscriberCommand { UserProfile = new UserProfileDto() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.UserProfile);
    }
}
