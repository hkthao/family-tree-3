using Moq;
using backend.Application.Events.Queries.GetUpcomingEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.Events.Queries; // For EventDto

namespace backend.Application.UnitTests.Events.Queries.GetUpcomingEvents
{
    public class GetUpcomingEventsQueryHandlerTests : TestBase
    {
        private readonly Mock<IPrivacyService> _mockPrivacyService;

        public GetUpcomingEventsQueryHandlerTests()
        {
            _mockPrivacyService = new Mock<IPrivacyService>();
            // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
            _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<EventDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto dto, Guid familyId, CancellationToken token) => dto);
            _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<EventDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<EventDto> dtos, Guid familyId, CancellationToken token) => dtos);

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
                Event.CreateSolarEvent("Past Event", "EVT1", EventType.Other, DateTime.UtcNow.AddYears(-10), RepeatRule.None, familyId),
                Event.CreateSolarEvent("Upcoming Event 1", "EVT2", EventType.Other, DateTime.UtcNow.AddDays(5), RepeatRule.None, familyId),
                Event.CreateSolarEvent("Upcoming Event 2", "EVT3", EventType.Other, DateTime.UtcNow.AddDays(10), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true); // Explicitly set admin for this test

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new GetUpcomingEventsQuery { FamilyId = familyId };

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
                Event.CreateSolarEvent("Accessible Event", "EVT1", EventType.Other, DateTime.UtcNow.AddDays(5), RepeatRule.None, accessibleFamily.Id),
                Event.CreateSolarEvent("Inaccessible Event", "EVT2", EventType.Other, DateTime.UtcNow.AddDays(5), RepeatRule.None, otherFamilyId) // Inaccessible family events
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new GetUpcomingEventsQuery { FamilyId = accessibleFamily.Id };

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
                Event.CreateSolarEvent("Upcoming Event 1", "EVT1", EventType.Other, DateTime.UtcNow.AddDays(5), RepeatRule.None, familyId),
                Event.CreateSolarEvent("Upcoming Event 2", "EVT2", EventType.Other, DateTime.UtcNow.AddDays(10), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new GetUpcomingEventsQuery { FamilyId = familyId };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull().And.BeEmpty();
        }
    }
}
