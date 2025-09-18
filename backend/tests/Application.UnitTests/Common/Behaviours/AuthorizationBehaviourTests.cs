using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Behaviours;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Security;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Common.Behaviours;

public class AuthorizationBehaviourTests
{
    private readonly Mock<IUser> _userMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;
    private readonly AuthorizationBehaviour<TestRequest, string> _behaviour;

    public AuthorizationBehaviourTests()
    {
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
        _behaviour = new AuthorizationBehaviour<TestRequest, string>(_userMock.Object, _identityServiceMock.Object);

        _nextMock.Setup(n => n()).ReturnsAsync("next called");
    }

    // A dummy request class for testing
    private class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoAuthorizeAttributePresent()
    {
        // Arrange
        var request = new TestRequest();

        // Act
        var result = await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("next called");
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenAuthorizeAttributePresentAndUserNotAuthenticated()
    {
        // Arrange
        var request = new TestRequestWithAuthorizeAttribute();
        _userMock.Setup(u => u.Id).Returns(default(string)); // Simulate unauthenticated user

        // Act
        Func<Task> act = async () => await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenUserIsInRequiredRole()
    {
        // Arrange
        var request = new TestRequestWithRolesAttribute();
        _userMock.Setup(u => u.Id).Returns("testuser"); // Authenticated user
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Administrator" }); // User is in Administrator role

        // Act
        var result = await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("next called");
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotInRequiredRole()
    {
        // Arrange
        var request = new TestRequestWithRolesAttribute();
        _userMock.Setup(u => u.Id).Returns("testuser"); // Authenticated user
        _userMock.Setup(u => u.Roles).Returns(new List<string> { "Guest" }); // User is NOT in Administrator role

        // Act
        Func<Task> act = async () => await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
        _nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenUserIsAuthorizedByPolicy()
    {
        // Arrange
        var request = new TestRequestWithPolicyAttribute();
        _userMock.Setup(u => u.Id).Returns("testuser"); // Authenticated user
        _identityServiceMock.Setup(i => i.AuthorizeAsync("testuser", "CanPurge")).ReturnsAsync(true);

        // Act
        var result = await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("next called");
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAuthorizedByPolicy()
    {
        // Arrange
        var request = new TestRequestWithPolicyAttribute();
        _userMock.Setup(u => u.Id).Returns("testuser"); // Authenticated user
        _identityServiceMock.Setup(i => i.AuthorizeAsync("testuser", "CanPurge")).ReturnsAsync(false); // Not authorized

        // Act
        Func<Task> act = async () => await _behaviour.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
        _nextMock.Verify(n => n(), Times.Never);
    }

    [Authorize(Policy = "CanPurge")]
    private class TestRequestWithPolicyAttribute : TestRequest { }

    [Authorize(Roles = "Administrator")]
    private class TestRequestWithRolesAttribute : TestRequest { }

    // A dummy request class with AuthorizeAttribute for testing
    [Authorize]
    private class TestRequestWithAuthorizeAttribute : TestRequest { }
}
