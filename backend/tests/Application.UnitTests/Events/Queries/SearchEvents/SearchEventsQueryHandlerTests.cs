using AutoMapper;
using backend.Application.Events.Queries.SearchEvents;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Events.Queries.SearchEvents;

public class SearchEventsQueryHandlerTests : IDisposable
{
    private readonly SearchEventsQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchEventsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new SearchEventsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Matching_Events()
    {
        // Arrange
        _context.Events.AddRange(
            new Event { Name = "Birthday Party", Description = "A fun party" },
            new Event { Name = "Wedding Anniversary", Description = "A special day" },
            new Event { Name = "Graduation Ceremony", Description = "A big achievement" }
        );
        await _context.SaveChangesAsync();

        var query = new SearchEventsQuery { Keyword = "Party" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.Should().Contain(x => x.Name == "Birthday Party");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
