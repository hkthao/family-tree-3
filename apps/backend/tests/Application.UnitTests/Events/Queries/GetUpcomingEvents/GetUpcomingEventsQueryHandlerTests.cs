using backend.Application.Events.Queries.GetUpcomingEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetUpcomingEvents
{
    public class GetUpcomingEventsQueryHandlerTests : TestBase
    {
        public GetUpcomingEventsQueryHandlerTests()
        {
            // TestBase already sets up _mockUser and _mockAuthorizationService
            // Set default authenticated user for specific scenarios if needed
            _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Default to admin for these tests to easily access events
        }

        [Fact]
        public async Task Handle_ShouldReturnUpcomingEvents_ForAdminUser()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                new Event("Past Event", "EVT1", EventType.Other, familyId, DateTime.UtcNow.AddDays(-10)),
                new Event("Upcoming Event 1", "EVT2", EventType.Other, familyId, DateTime.UtcNow.AddDays(5)),
                new Event("Upcoming Event 2", "EVT3", EventType.Other, familyId, DateTime.UtcNow.AddDays(10))
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Explicitly set admin for this test

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
            var query = new GetUpcomingEventsQuery { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(15) };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnUpcomingEvents_ForNonAdminUserWithAccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherFamilyId = Guid.NewGuid(); // This will be the ID for an inaccessible family

            // Set up _mockUser with specific UserId and authenticated status
            _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUser.Setup(x => x.UserId).Returns(userId);
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Explicitly set non-admin for this test

            // Create an accessible family and associate the user with it
            var accessibleFamily = new Family { Id = Guid.NewGuid(), Name = "Accessible Family", Code = "ACC", Visibility = "Private" };
            accessibleFamily.AddFamilyUser(userId, FamilyRole.Viewer);
            _context.Families.Add(accessibleFamily);
            await _context.SaveChangesAsync(CancellationToken.None); // Save accessible family to get its Id

            _context.Events.AddRange(new List<Event>
            {
                new Event("Accessible Event", "EVT1", EventType.Other, accessibleFamily.Id, DateTime.UtcNow.AddDays(5)),
                new Event("Inaccessible Event", "EVT2", EventType.Other, otherFamilyId, DateTime.UtcNow.AddDays(5)) // Inaccessible family events
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
            var query = new GetUpcomingEventsQuery { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(15) };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(1);
            if (result.Value != null)
            {
                result.Value.First().Name.Should().Be("Accessible Event");
            }
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _mockUser.Setup(x => x.IsAuthenticated).Returns(false); // Simulate unauthenticated user
            _mockUser.Setup(x => x.UserId).Returns(Guid.Empty); // Ensure UserId is empty for unauthenticated
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false); // Not admin, but it won't be checked

            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                new Event("Upcoming Event 1", "EVT1", EventType.Other, familyId, DateTime.UtcNow.AddDays(5)),
                new Event("Upcoming Event 2", "EVT2", EventType.Other, familyId, DateTime.UtcNow.AddDays(10))
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
            var query = new GetUpcomingEventsQuery { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(15) };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull().And.BeEmpty();
        }
    }
}
