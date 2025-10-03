using AutoMapper;
using backend.Application.Events.Commands.UpdateEvent;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : IDisposable
{
    private readonly UpdateEventCommandHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateEventCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new UpdateEventCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Update_Event()
    {
        // Arrange
        var existingEvent = new Event { Name = "Old Name", Description = "Old Description" };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        var command = new UpdateEventCommand
        {
            Id = existingEvent.Id,
            Name = "New Name",
            Description = "New Description",
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedEvent = await _context.Events.FindAsync(existingEvent.Id);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be("New Name");
        updatedEvent.Description.Should().Be("New Description");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}