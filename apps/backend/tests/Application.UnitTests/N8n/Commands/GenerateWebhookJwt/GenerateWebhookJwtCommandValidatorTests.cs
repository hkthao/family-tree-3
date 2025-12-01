using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.N8n.Commands.GenerateWebhookJwt;

public class GenerateWebhookJwtCommandValidatorTests
{
    private readonly GenerateWebhookJwtCommandValidator _validator;

    public GenerateWebhookJwtCommandValidatorTests()
    {
        _validator = new GenerateWebhookJwtCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenSubjectIsEmpty()
    {
        var command = new GenerateWebhookJwtCommand { Subject = string.Empty, ExpiresInMinutes = 30 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject là bắt buộc.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenSubjectIsSpecified()
    {
        var command = new GenerateWebhookJwtCommand { Subject = "test_subject", ExpiresInMinutes = 30 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Subject);
    }

    [Fact]
    public void ShouldHaveError_WhenExpiresInMinutesIsZero()
    {
        var command = new GenerateWebhookJwtCommand { Subject = "test_subject", ExpiresInMinutes = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiresInMinutes)
            .WithErrorMessage("Thời gian hết hạn phải lớn hơn 0 phút.");
    }

    [Fact]
    public void ShouldHaveError_WhenExpiresInMinutesIsNegative()
    {
        var command = new GenerateWebhookJwtCommand { Subject = "test_subject", ExpiresInMinutes = -1 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiresInMinutes)
            .WithErrorMessage("Thời gian hết hạn phải lớn hơn 0 phút.");
    }

    [Fact]
    public void ShouldHaveError_WhenExpiresInMinutesIsGreaterThan1440()
    {
        var command = new GenerateWebhookJwtCommand { Subject = "test_subject", ExpiresInMinutes = 1441 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ExpiresInMinutes)
            .WithErrorMessage("Thời gian hết hạn không được vượt quá 1440 phút (24 giờ).");
    }

    [Fact]
    public void ShouldNotHaveError_WhenExpiresInMinutesIsValid()
    {
        var command = new GenerateWebhookJwtCommand { Subject = "test_subject", ExpiresInMinutes = 120 };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiresInMinutes);
    }
}
