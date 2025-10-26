using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.SearchEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Queries.SearchEvents;

public class SearchEventsQueryHandlerTests : TestBase
{
    private readonly SearchEventsQueryHandler _handler;

    public SearchEventsQueryHandlerTests()
    {
        _handler = new SearchEventsQueryHandler(
            _context,
            _mapper
        );
    }
}
