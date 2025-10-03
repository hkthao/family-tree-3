using backend.Application.Common.Exceptions;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests
{
    private readonly UpdateEventCommandHandler _handler;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IMemberRepository> _mockMemberRepository;

    public UpdateEventCommandHandlerTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockMemberRepository = new Mock<IMemberRepository>();
        _handler = new UpdateEventCommandHandler(_mockEventRepository.Object, _mockMemberRepository.Object);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Name = "Test Event", FamilyId = Guid.NewGuid() };
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(ev);
        _mockEventRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
        _mockMemberRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Member>());

        var command = new UpdateEventCommand
        {
            Id = eventId,
            Name = "Updated Name",
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventRepository.Verify(repo => repo.UpdateAsync(It.Is<Event>(e => e.Id == eventId && e.Name == command.Name)), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = Guid.NewGuid(), Name = "test" };
        _mockEventRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((Event?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
