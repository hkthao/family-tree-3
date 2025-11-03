using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Events;
using backend.Application.Events.Queries.GetUpcomingEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetUpcomingEvents
{
    public class GetUpcomingEventsQueryHandlerTests : TestBase
    {
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<ICurrentUser> _currentUserMock;

        public GetUpcomingEventsQueryHandlerTests()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _currentUserMock = new Mock<ICurrentUser>();
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

            _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _authorizationServiceMock.Object, _currentUserMock.Object);
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
            var familyId = Guid.NewGuid();
            var otherFamilyId = Guid.NewGuid();

            _context.FamilyUsers.Add(new FamilyUser(familyId, userId, FamilyRole.Viewer));
            _context.Events.AddRange(new List<Event>
            {
                new Event("Accessible Event", "EVT1", EventType.Other, familyId, DateTime.UtcNow.AddDays(5)),
                new Event("Inaccessible Event", "EVT2", EventType.Other, otherFamilyId, DateTime.UtcNow.AddDays(5))
            });
            await _context.SaveChangesAsync(CancellationToken.None);

            _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
            _currentUserMock.Setup(x => x.UserId).Returns(userId);

            var handler = new GetUpcomingEventsQueryHandler(_context, _mapper, _authorizationServiceMock.Object, _currentUserMock.Object);
            var query = new GetUpcomingEventsQuery { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(15) };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(1);
            if (result.Value != null) {
                result.Value.First().Name.Should().Be("Accessible Event");
            }
        }
    }
}