using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly CreateEventCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;

    public CreateEventCommandHandlerTests()
    {
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _handler = new CreateEventCommandHandler(_context, _mockAuthorizationService.Object);
    }


}
