using AutoMapper;
using backend.Application.Events.Queries.GetEventsByIds;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Domain.Entities;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile

namespace backend.Application.UnitTests.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandlerTests : IDisposable
{
    private readonly GetEventsByIdsQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsByIdsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetEventsByIdsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Matching_Events()
    {
        // Arrange
        var event1 = new Event { Name = "Event 1" };
        var event2 = new Event { Name = "Event 2" };
        var event3 = new Event { Name = "Event 3" };
        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync();

        var query = new GetEventsByIdsQuery(new List<Guid> { event1.Id, event3.Id });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Event 1");
        result.Should().Contain(x => x.Name == "Event 3");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
