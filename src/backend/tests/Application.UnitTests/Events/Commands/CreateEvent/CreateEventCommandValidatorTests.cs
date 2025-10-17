using backend.Application.Events.Commands.CreateEvent;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Name' của CreateEventCommand là một chuỗi rỗng.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'Name' vượt quá 200 ký tự.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'Name' của CreateEventCommand
    /// có độ dài lớn hơn giới hạn 200 ký tự.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenNameExceeds200Characters()
    {
        // Arrange
        var command = new CreateEventCommand { Name = new string('A', 201) };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi trường 'Name' hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ không báo lỗi khi trường 'Name' của CreateEventCommand
    /// có giá trị hợp lệ (không rỗng, không vượt quá giới hạn ký tự).
    /// </remarks>
    [Fact]
    public void ShouldNotHaveErrorWhenNameIsValid()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Tên Sự Kiện Hợp Lệ" };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi trường 'FamilyId' rỗng (Guid.Empty).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi trường 'FamilyId' của CreateEventCommand
    /// là một Guid rỗng, vì FamilyId là bắt buộc.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenFamilyIdIsEmpty()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Valid Name", FamilyId = null };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FamilyId);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi 'EndDate' trước 'StartDate'.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng validator sẽ báo lỗi khi 'EndDate' của sự kiện được đặt trước 'StartDate',
    /// một logic nghiệp vụ quan trọng cho các sự kiện.
    /// </remarks>
    [Fact]
    public void ShouldHaveErrorWhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "Sự kiện với ngày không hợp lệ",
            FamilyId = Guid.NewGuid(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(-1) // EndDate trước StartDate
        };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }
}
