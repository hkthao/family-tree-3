using AutoMapper;
using backend.Application.Events.Queries.GetEvents;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Application.UnitTests.Common;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile

namespace backend.Application.UnitTests.Events.Queries.GetEvents;

public class GetEventsQueryHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetEventsQueryHandler _handler;

    public GetEventsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetEventsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
    {
        // Arrange
        var familyId = _context.Families.First().Id;
        var countBefore = await _context.Events.Where(e => e.FamilyId == familyId).CountAsync();
        var events = new List<Event>
        {
            new Event { Name = "Event 1", FamilyId = familyId, Created = DateTime.UtcNow },
            new Event { Name = "Event 2", FamilyId = familyId, Created = DateTime.UtcNow }
        };
        _context.Events.AddRange(events);
        _context.SaveChanges();

        // Act
        var result = await _handler.Handle(new GetEventsQuery { FamilyId = familyId, ItemsPerPage = 10000 }, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(countBefore + 2);
        result.Value.Should().ContainEquivalentOf(new { Name = "Event 1" });
        result.Value.Should().ContainEquivalentOf(new { Name = "Event 2" });
    }
}
