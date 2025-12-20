using backend.Application.Events.Queries.GetAllEventsByFamilyId;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetAllEventsByFamilyId
{
    public class GetAllEventsByFamilyIdQueryHandlerTests : TestBase
    {
        public GetAllEventsByFamilyIdQueryHandlerTests()
        {
            // Setup authenticated user and admin roles for tests by default
            _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllEventsForGivenFamilyId()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                Event.CreateSolarEvent("Birthday", "Bday", EventType.Birth, DateTime.UtcNow.Date.AddYears(-30), RepeatRule.Yearly, familyId),
                Event.CreateSolarEvent("Anniversary", "Aniv", EventType.Marriage, DateTime.UtcNow.Date.AddYears(-10), RepeatRule.Yearly, familyId),
                Event.CreateSolarEvent("Meeting", "Mtg", EventType.Other, DateTime.UtcNow.Date.AddDays(5), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetAllEventsByFamilyIdQueryHandler(_context, _mapper);
            var query = new GetAllEventsByFamilyIdQuery { FamilyId = familyId };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(3);
            result.Value!.All(e => e.FamilyId == familyId).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyListWhenNoEventsForFamilyId()
        {
            // Arrange
            var familyId = Guid.NewGuid(); // A familyId with no events
            var anotherFamilyId = Guid.NewGuid();

            _context.Events.AddRange(new List<Event>
            {
                Event.CreateSolarEvent("Event 1", "E1", EventType.Other, DateTime.UtcNow.Date, RepeatRule.None, anotherFamilyId),
                Event.CreateSolarEvent("Event 2", "E2", EventType.Other, DateTime.UtcNow.Date, RepeatRule.None, anotherFamilyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetAllEventsByFamilyIdQueryHandler(_context, _mapper);
            var query = new GetAllEventsByFamilyIdQuery { FamilyId = familyId };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldNotReturnEventsFromOtherFamilies()
        {
            // Arrange
            var targetFamilyId = Guid.NewGuid();
            var otherFamilyId = Guid.NewGuid();

            _context.Events.AddRange(new List<Event>
            {
                // Events for the target family
                Event.CreateSolarEvent("Target Event 1", "TE1", EventType.Birth, DateTime.UtcNow.Date, RepeatRule.None, targetFamilyId),
                Event.CreateSolarEvent("Target Event 2", "TE2", EventType.Death, DateTime.UtcNow.Date, RepeatRule.None, targetFamilyId),
                // Events for another family
                Event.CreateSolarEvent("Other Event 1", "OE1", EventType.Other, DateTime.UtcNow.Date, RepeatRule.None, otherFamilyId),
                Event.CreateSolarEvent("Other Event 2", "OE2", EventType.Other, DateTime.UtcNow.Date, RepeatRule.None, otherFamilyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetAllEventsByFamilyIdQueryHandler(_context, _mapper);
            var query = new GetAllEventsByFamilyIdQuery { FamilyId = targetFamilyId };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(2); // Only events from targetFamilyId
            result.Value!.All(e => e.FamilyId == targetFamilyId).Should().BeTrue();
            result.Value!.Any(e => e.FamilyId == otherFamilyId).Should().BeFalse();
        }
    }
}
