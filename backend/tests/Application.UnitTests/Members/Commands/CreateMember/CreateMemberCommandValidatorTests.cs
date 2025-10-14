using backend.Application.Members.Commands.CreateMember;
using FluentValidation.TestHelper;
using Xunit;
using System;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator;

    public CreateMemberCommandValidatorTests()
    {
        _validator = new CreateMemberCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem một lệnh tạo thành viên hợp lệ có không có lỗi xác thực.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi tất cả các trường bắt buộc và định dạng đều hợp lệ,
    /// validator sẽ không báo cáo bất kỳ lỗi nào.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveValidationErrorForValidCommand()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FirstName = "Thành viên",
            LastName = "Hợp lệ",
            FamilyId = Guid.NewGuid(),
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = new DateTime(2050, 1, 1),
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'FirstName' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'FirstName' của CreateMemberCommand là một chuỗi rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "", LastName = "Hợp lệ", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'FirstName' vượt quá 100 ký tự.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'FirstName' của CreateMemberCommand
    /// có độ dài lớn hơn giới hạn 100 ký tự.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenFirstNameExceeds100Characters()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = new string('A', 101), LastName = "Hợp lệ", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi trường 'FirstName' hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ không báo lỗi khi trường 'FirstName' của CreateMemberCommand
    /// có giá trị hợp lệ.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveErrorWhenFirstNameIsValid()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "Hợp lệ", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'LastName' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'LastName' của CreateMemberCommand là một chuỗi rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenLastNameIsEmpty()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'LastName' vượt quá 100 ký tự.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'LastName' của CreateMemberCommand
    /// có độ dài lớn hơn giới hạn 100 ký tự.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenLastNameExceeds100Characters()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = new string('A', 101), FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi trường 'LastName' hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ không báo lỗi khi trường 'LastName' của CreateMemberCommand
    /// có giá trị hợp lệ.
    /// </remarks>
    [Fact]
    public void ShouldNotHaveErrorWhenLastNameIsValid()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "Hợp lệ", FamilyId = Guid.NewGuid() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'FamilyId' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'FamilyId' của CreateMemberCommand là một Guid rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenFamilyIdIsEmpty()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "Hợp lệ", FamilyId = Guid.Empty };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyId);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Gender' không hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Gender' của CreateMemberCommand
    /// không phải là "Male", "Female", hoặc "Other".
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenGenderIsInvalid()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "Hợp lệ", FamilyId = Guid.NewGuid(), Gender = "Invalid" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi 'DateOfDeath' trước 'DateOfBirth'.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi 'DateOfDeath' của thành viên được đặt trước 'DateOfBirth'.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenDateOfDeathIsBeforeDateOfBirth()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FirstName = "Hợp lệ",
            LastName = "Hợp lệ",
            FamilyId = Guid.NewGuid(),
            DateOfBirth = new DateTime(2000, 1, 1),
            DateOfDeath = new DateTime(1990, 1, 1) // DateOfDeath trước DateOfBirth
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfDeath);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi 'AvatarUrl' không hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi 'AvatarUrl' của thành viên không phải là một URL hợp lệ.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenAvatarUrlIsInvalid()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "Hợp lệ", LastName = "Hợp lệ", FamilyId = Guid.NewGuid(), AvatarUrl = "invalid-url" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl);
    }
}
