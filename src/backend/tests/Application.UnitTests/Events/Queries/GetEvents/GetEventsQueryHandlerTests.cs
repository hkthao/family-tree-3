using backend.Application.Events.Queries.GetEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEvents
{
    public class GetEventsQueryHandlerTests : TestBase
    {
        public GetEventsQueryHandlerTests()
        {
        }

        [Fact]
        public async Task Handle_ShouldReturnAllEvents_WhenNoFilterIsApplied()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                new Event("Event 1", "EVT1", EventType.Birth, familyId),
                new Event("Event 2", "EVT2", EventType.Death, familyId),
                new Event("Event 3", "EVT3", EventType.Marriage, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetEventsQueryHandler(_context, _mapper);
            var query = new GetEventsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(3);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredEvents_WhenSearchTermIsProvided()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                new Event("Birthday Party", "EVT1", EventType.Birth, familyId),
                new Event("Wedding Anniversary", "EVT2", EventType.Marriage, familyId),
                new Event("Funeral", "EVT3", EventType.Death, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetEventsQueryHandler(_context, _mapper);
            var query = new GetEventsQuery { SearchTerm = "Party" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.Should().HaveCount(1);
                result.Value.First().Name.Should().Be("Birthday Party");
            }
        }
    }
}