using FluentValidation.TestHelper;
using Xunit;
using backend.Application.Events.Commands.CreateEvents;
using System;
using System.Collections.Generic;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventsCommandValidatorTests
{
    private readonly CreateEventsCommandValidator _validator;

    public CreateEventsCommandValidatorTests()
    {
        _validator = new CreateEventsCommandValidator();
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi danh sách sự kiện rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenEventsListIsEmpty()
    {
        // Arrange
        var command = new CreateEventsCommand(new List<CreateEventDto>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Events);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi một sự kiện trong danh sách có tên rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAnEventInListHasEmptyName()
    {
        // Arrange
        var eventsToCreate = new List<CreateEventDto>
        {
            new CreateEventDto
            {
                Name = "",
                Code = "EVT001",
                Type = EventType.Birth,
                FamilyId = Guid.NewGuid()
            }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Events[0].Name);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi một sự kiện trong danh sách có FamilyId rỗng.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAnEventInListHasEmptyFamilyId()
    {
        // Arrange
        var eventsToCreate = new List<CreateEventDto>
        {
            new CreateEventDto
            {
                Name = "Valid Name",
                Code = "EVT001",
                Type = EventType.Birth,
                FamilyId = null
            }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Events[0].FamilyId);
    }

    /// <summary>
    /// Kiểm tra xem có lỗi xác thực khi EndDate của một sự kiện trong danh sách trước StartDate.
    /// </summary>
    [Fact]
    public void ShouldHaveErrorWhenAnEventInListHasEndDateBeforeStartDate()
    {
        // Arrange
        var eventsToCreate = new List<CreateEventDto>
        {
            new CreateEventDto
            {
                Name = "Valid Name",
                Code = "EVT001",
                Type = EventType.Birth,
                FamilyId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1)
            }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Events[0].EndDate);
    }

    /// <summary>
    /// Kiểm tra xem không có lỗi xác thực khi danh sách sự kiện hợp lệ.
    /// </summary>
    [Fact]
    public void ShouldNotHaveErrorWhenEventsListIsValid()
    {
        // Arrange
        var eventsToCreate = new List<CreateEventDto>
        {
            new CreateEventDto
            {
                Name = "Valid Event 1",
                Code = "EVT001",
                Type = EventType.Birth,
                FamilyId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            },
            new CreateEventDto
            {
                Name = "Valid Event 2",
                Code = "EVT002",
                Type = EventType.Death,
                FamilyId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow.AddDays(5)
            }
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
