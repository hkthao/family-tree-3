using backend.Application.Events.Queries.GetEventsByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetEventsByIds
{
    public class GetEventsByIdsQueryHandlerTests : TestBase
    {
        public GetEventsByIdsQueryHandlerTests()
        {
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectEvents_WhenGivenValidIds()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var event1 = new Event("Event 1", "EVT1", EventType.Birth, familyId);
            var event2 = new Event("Event 2", "EVT2", EventType.Death, familyId);
            var event3 = new Event("Event 3", "EVT3", EventType.Marriage, familyId);
            _context.Events.AddRange(event1, event2, event3);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetEventsByIdsQueryHandler(_context, _mapper);
            var query = new GetEventsByIdsQuery(new List<Guid> { event1.Id, event3.Id });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.Should().HaveCount(2);
                result.Value.Select(e => e.Id).Should().Contain(event1.Id);
                result.Value.Select(e => e.Id).Should().Contain(event3.Id);
            }
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenGivenNoIds()
        {
            // Arrange
            var handler = new GetEventsByIdsQueryHandler(_context, _mapper);
            var query = new GetEventsByIdsQuery(new List<Guid>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenGivenNonExistentIds()
        {
            // Arrange
            var handler = new GetEventsByIdsQueryHandler(_context, _mapper);
            var query = new GetEventsByIdsQuery(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }
    }
}
