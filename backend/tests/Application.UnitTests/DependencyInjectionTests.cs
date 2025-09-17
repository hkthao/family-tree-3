using backend.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using FluentAssertions;
using Moq;
using MediatR;
using AutoMapper;

namespace backend.Application.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostApplicationBuilderMock = new Mock<IHostApplicationBuilder>();
        hostApplicationBuilderMock.Setup(b => b.Services).Returns(services);

        // Act
        hostApplicationBuilderMock.Object.AddApplicationServices();

        // Assert
        services.Should().NotBeNull();
        // Add more specific assertions for services you expect to be registered
        // For example, check for MediatR, AutoMapper, FluentValidation
        services.Should().Contain(s => s.ServiceType == typeof(IMediator));
        services.Should().Contain(s => s.ServiceType == typeof(IMapper));
    }
}
