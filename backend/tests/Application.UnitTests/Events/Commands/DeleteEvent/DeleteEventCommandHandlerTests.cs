using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;

    public DeleteEventCommandHandlerTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = Guid.NewGuid() };
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _mockEventRepository.Setup(repo => repo.DeleteAsync(eventId)).Returns(Task.CompletedTask);

        var command = new DeleteEventCommand(eventId);
        var handler = new DeleteEventCommandHandler(_mockEventRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(repo => repo.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(invalidId)).ReturnsAsync((Event?)null);

        var command = new DeleteEventCommand(invalidId);
        var handler = new DeleteEventCommandHandler(_mockEventRepository.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
