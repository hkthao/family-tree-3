using backend.Application.Families.Commands.UpdateFamilyLimitConfiguration;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Tests.Application.UnitTests.Families.Commands.UpdateFamilyLimitConfiguration;

public class UpdateFamilyLimitConfigurationCommandValidatorTests
{
    private readonly UpdateFamilyLimitConfigurationCommandValidator _validator;

    public UpdateFamilyLimitConfigurationCommandValidatorTests()
    {
        _validator = new UpdateFamilyLimitConfigurationCommandValidator();
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenFamilyIdIsEmpty()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.Empty,
            MaxMembers = 10,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FamilyId)
              .WithErrorMessage("ID gia đình là bắt buộc.");
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenMaxMembersIsZero()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 0,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MaxMembers)
              .WithErrorMessage("Số lượng thành viên tối đa phải lớn hơn 0.");
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenMaxMembersIsNegative()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = -1,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MaxMembers)
              .WithErrorMessage("Số lượng thành viên tối đa phải lớn hơn 0.");
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenMaxStorageMbIsZero()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 10,
            MaxStorageMb = 0,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MaxStorageMb)
              .WithErrorMessage("Dung lượng lưu trữ tối đa phải lớn hơn 0.");
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenMaxStorageMbIsNegative()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 10,
            MaxStorageMb = -1,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.MaxStorageMb)
              .WithErrorMessage("Dung lượng lưu trữ tối đa phải lớn hơn 0.");
    }

    [Fact]
    public void ShouldHaveValidationErrorWhenAiChatMonthlyLimitIsNegative()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 10,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = -10
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AiChatMonthlyLimit)
              .WithErrorMessage("Giới hạn trò chuyện AI hàng tháng không thể âm.");
    }

    [Fact]
    public void ShouldNotHaveValidationErrorWhenCommandIsValid()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 10,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = 100
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveValidationErrorWhenAiChatMonthlyLimitIsZero()
    {
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(),
            MaxMembers = 10,
            MaxStorageMb = 1024,
            AiChatMonthlyLimit = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
