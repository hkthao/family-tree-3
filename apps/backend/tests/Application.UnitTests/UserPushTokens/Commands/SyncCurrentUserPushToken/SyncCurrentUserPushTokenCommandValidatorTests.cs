using backend.Application.UserPushTokens.Commands.SyncCurrentUserPushToken;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.UserPushTokens.Commands.SyncCurrentUserPushToken;

public class SyncCurrentUserPushTokenCommandValidatorTests
{
    private readonly SyncCurrentUserPushTokenCommandValidator _validator;

    public SyncCurrentUserPushTokenCommandValidatorTests()
    {
        _validator = new SyncCurrentUserPushTokenCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenExpoPushTokenIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi ExpoPushToken là rỗng.
        var command = new SyncCurrentUserPushTokenCommand(string.Empty, "android", "device123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpoPushToken)
              .WithErrorMessage("ExpoPushToken không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenPlatformIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Platform là rỗng.
        var command = new SyncCurrentUserPushTokenCommand("token123", string.Empty, "device123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Platform)
              .WithErrorMessage("Platform không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenDeviceIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi DeviceId là rỗng.
        var command = new SyncCurrentUserPushTokenCommand("token123", "android", string.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
              .WithErrorMessage("DeviceId không được để trống.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi tất cả các trường đều hợp lệ.
        var command = new SyncCurrentUserPushTokenCommand("token123", "android", "device123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
