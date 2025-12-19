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
            // TestBase already sets up _mockUser and _mockAuthorizationService
            // Set default authenticated user for specific scenarios if needed
            _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Default to admin for these tests to easily access events
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectEvents_WhenGivenValidIds()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var event1 = Event.CreateSolarEvent("Event 1", "EVT1", EventType.Birth, DateTime.UtcNow.AddDays(-10), RepeatRule.None, familyId);
            var event2 = Event.CreateSolarEvent("Event 2", "EVT2", EventType.Death, DateTime.UtcNow.AddDays(-5), RepeatRule.None, familyId);
            var event3 = Event.CreateSolarEvent("Event 3", "EVT3", EventType.Marriage, DateTime.UtcNow.AddDays(0), RepeatRule.None, familyId);
            _context.Events.AddRange(event1, event2, event3);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetEventsByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
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
            var handler = new GetEventsByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
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
            var handler = new GetEventsByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
            var query = new GetEventsByIdsQuery(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _mockUser.Setup(x => x.IsAuthenticated).Returns(false); // Simulate unauthenticated user
            _mockUser.Setup(x => x.UserId).Returns(Guid.Empty); // Ensure UserId is empty for unauthenticated
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Not admin, but it won't be checked

            var familyId = Guid.NewGuid();
            var event1 = Event.CreateSolarEvent("Event 1", "EVT1", EventType.Birth, DateTime.UtcNow.AddDays(5), RepeatRule.None, familyId);
            _context.Events.Add(event1);
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetEventsByIdsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
            var query = new GetEventsByIdsQuery(new List<Guid> { event1.Id });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull().And.BeEmpty();
        }
    }
}
