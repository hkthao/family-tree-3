using backend.Application.Events.Commands.CreateEvents;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventDtoValidatorTests
{
    private readonly CreateEventDtoValidator _validator;

    public CreateEventDtoValidatorTests()
    {
        _validator = new CreateEventDtoValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name là chuỗi rỗng.
        var dto = new CreateEventDto { Name = string.Empty, Code = "CODE", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tên sự kiện không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Name vượt quá 200 ký tự.
        var dto = new CreateEventDto { Name = new string('a', 201), Code = "CODE", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Tên sự kiện không được vượt quá 200 ký tự.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenNameIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Name hợp lệ.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenCodeIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Code là chuỗi rỗng.
        var dto = new CreateEventDto { Name = "Valid Name", Code = string.Empty, FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Mã sự kiện không được để trống.");
    }

    [Fact]
    public void ShouldHaveError_WhenCodeExceedsMaxLength()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi Code vượt quá 50 ký tự.
        var dto = new CreateEventDto { Name = "Valid Name", Code = new string('a', 51), FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code)
              .WithErrorMessage("Mã sự kiện không được vượt quá 50 ký tự.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCodeIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi Code hợp lệ.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "VALIDCODE", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ShouldHaveError_WhenFamilyIdIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi FamilyId là Guid rỗng.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FamilyId)
              .WithErrorMessage("ID gia đình không được để trống.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenFamilyIdIsValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi FamilyId hợp lệ.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.FamilyId);
    }

    [Fact]
    public void ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi EndDate trước StartDate.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now.AddDays(1), EndDate = DateTime.Now };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("Ngày kết thúc không được trước ngày bắt đầu.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsAfterStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate sau StartDate.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldNotHaveError_WhenEndDateIsSameAsStartDate()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi EndDate bằng StartDate.
        var dto = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = DateTime.Now };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldNotHaveError_WhenStartDateOrEndDateIsNull()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi StartDate hoặc EndDate là null.
        var dto1 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = DateTime.Now };
        var result1 = _validator.TestValidate(dto1);
        result1.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        var dto2 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = DateTime.Now, EndDate = null };
        var result2 = _validator.TestValidate(dto2);
        result2.ShouldNotHaveValidationErrorFor(x => x.EndDate);

        var dto3 = new CreateEventDto { Name = "Valid Name", Code = "CODE", FamilyId = Guid.NewGuid(), StartDate = null, EndDate = null };
        var result3 = _validator.TestValidate(dto3);
        result3.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}
