using backend.Application.Events.Queries.SearchEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.SearchEvents
{
    public class SearchEventsQueryHandlerTests : TestBase
    {
        public SearchEventsQueryHandlerTests()
        {
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
                new Event("Event 1", "EVT1", EventType.Birth, familyId),
                new Event("Event 2", "EVT2", EventType.Death, familyId),
                new Event("Event 3", "EVT3", EventType.Marriage, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
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
                new Event("Birthday Party", "EVT1", EventType.Birth, familyId),
                new Event("Wedding Anniversary", "EVT2", EventType.Marriage, familyId),
                new Event("Funeral", "EVT3", EventType.Death, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
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
                new Event("Event 1", "EVT1", EventType.Birth, familyId),
                new Event("Event 2", "EVT2", EventType.Death, familyId)
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            var handler = new SearchEventsQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object);
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
