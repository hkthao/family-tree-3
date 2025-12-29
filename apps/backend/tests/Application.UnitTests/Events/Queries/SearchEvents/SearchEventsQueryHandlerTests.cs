using Moq;
using backend.Application.Events.Queries.SearchEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;
using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.Events.Queries; // Add this using statement for EventDto

namespace backend.Application.UnitTests.Events.Queries.SearchEvents
{
    public class SearchEventsQueryHandlerTests : TestBase
    {
        private readonly Mock<IPrivacyService> _mockPrivacyService;

        public SearchEventsQueryHandlerTests()
        {
            _mockPrivacyService = new Mock<IPrivacyService>();
            // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
            _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<EventDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto dto, Guid familyId, CancellationToken token) => dto);
            _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<EventDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<EventDto> dtos, Guid familyId, CancellationToken token) => dtos);

            // Set up authenticated user by default for most tests
            _mockUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
            // Default to admin for existing tests that don't explicitly set user permissions
            _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        }

        [Fact]
        public async Task Handle_ShouldReturnPaginatedListOfEvents_WhenCalled()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                Event.CreateSolarEvent("Event 1", "EVT1", EventType.Birth, DateTime.UtcNow.Date, RepeatRule.None, familyId),
                Event.CreateSolarEvent("Event 2", "EVT2", EventType.Death, DateTime.UtcNow.Date.AddDays(1), RepeatRule.None, familyId),
                Event.CreateSolarEvent("Event 3", "EVT3", EventType.Marriage, DateTime.UtcNow.Date.AddDays(2), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new SearchEventsQuery { Page = 1, ItemsPerPage = 2 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.Items.Should().HaveCount(2);
                result.Value.TotalItems.Should().Be(3);
            }
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredEvents_WhenSearchQueryIsProvided()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _context.Events.AddRange(new List<Event>
            {
                Event.CreateSolarEvent("Birthday Party", "EVT1", EventType.Birth, DateTime.UtcNow.Date, RepeatRule.None, familyId),
                Event.CreateSolarEvent("Wedding Anniversary", "EVT2", EventType.Marriage, DateTime.UtcNow.Date.AddDays(1), RepeatRule.None, familyId),
                Event.CreateSolarEvent("Funeral", "EVT3", EventType.Death, DateTime.UtcNow.Date.AddDays(2), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new SearchEventsQuery { SearchQuery = "Party", Page = 1, ItemsPerPage = 10 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.Items.Should().HaveCount(1);
                result.Value.Items.First().Name.Should().Be("Birthday Party");
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
                Event.CreateSolarEvent("Event 1", "EVT1", EventType.Birth, DateTime.UtcNow.Date, RepeatRule.None, familyId),
                Event.CreateSolarEvent("Event 2", "EVT2", EventType.Death, DateTime.UtcNow.Date.AddDays(1), RepeatRule.None, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
            var query = new SearchEventsQuery { Page = 1, ItemsPerPage = 2 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue(); // It should be success because it returns an empty list
            result.Value.Should().NotBeNull();
            if (result.Value != null)
            {
                result.Value.Items.Should().BeEmpty();
                result.Value.TotalItems.Should().Be(0);
                result.Value.TotalPages.Should().Be(0); // Assuming 0 total pages for empty list
            }
        }
    }
}
