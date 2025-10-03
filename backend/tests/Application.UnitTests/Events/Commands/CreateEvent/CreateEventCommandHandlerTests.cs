using backend.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IMemberRepository> _mockMemberRepository;

    public CreateEventCommandHandlerTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockMemberRepository = new Mock<IMemberRepository>();
    }

    [Fact]
    public async Task Handle_ShouldCreateEventAndReturnId()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "Test Event",
            StartDate = new DateTime(2023, 1, 1),
            FamilyId = Guid.NewGuid()
        };

        _mockEventRepository.Setup(repo => repo.AddAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event ev) =>
            {
                ev.Id = Guid.NewGuid(); // Simulate ID generation
                return ev;
            });
        _mockMemberRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Member>());

        var handler = new CreateEventCommandHandler(_mockEventRepository.Object, _mockMemberRepository.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockEventRepository.Verify(repo => repo.AddAsync(It.Is<Event>(e => e.Name == command.Name)), Times.Once);
    }
}
