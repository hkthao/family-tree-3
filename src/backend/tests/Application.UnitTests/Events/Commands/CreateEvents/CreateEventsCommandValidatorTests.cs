using backend.Application.Events.Commands.CreateEvents;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventsCommandValidatorTests
{
    private readonly CreateEventsCommandValidator _validator;

    public CreateEventsCommandValidatorTests()
    {
        _validator = new CreateEventsCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenEventsListIsEmpty()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi danh sách Events rỗng.
        var command = new CreateEventsCommand([]);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Events)
              .WithErrorMessage("Danh sách sự kiện không được để trống.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenEventsListIsNotEmptyAndValid()
    {
        // 🎯 Mục tiêu của test: Xác minh không có lỗi khi danh sách Events không rỗng và hợp lệ.
        var validEventDto = new CreateEventDto { Name = "Valid Event", Code = "EVT001", FamilyId = Guid.NewGuid() };
        var command = new CreateEventsCommand([validEventDto]);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenAnyEventDtoIsInvalid()
    {
        // 🎯 Mục tiêu của test: Xác minh lỗi khi bất kỳ CreateEventDto nào trong danh sách không hợp lệ.
        var invalidEventDto = new CreateEventDto { Name = string.Empty, Code = "EVT002", FamilyId = Guid.NewGuid() }; // Invalid name
        var validEventDto = new CreateEventDto { Name = "Valid Event", Code = "EVT001", FamilyId = Guid.NewGuid() };

        var command = new CreateEventsCommand([validEventDto, invalidEventDto]);
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse(); // Overall validation should fail
        result.Errors.Should().Contain(e => e.PropertyName == "Events[1].Name" && e.ErrorMessage == "Tên sự kiện không được để trống.");
    }
}
