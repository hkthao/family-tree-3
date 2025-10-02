using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Events.Queries.GetEvents;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();
        _handler = new GetEventsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _context.Events.AddRange(
            new Event { Name = "Event 1", FamilyId = familyId },
            new Event { Name = "Event 2", FamilyId = familyId }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetEventsQuery { FamilyId = familyId }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
