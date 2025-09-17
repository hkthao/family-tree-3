using backend.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Shouldly;
using Moq;

namespace backend.Application.UnitTests;

public class DependencyInjectionTests
{
    [Test]
    public void AddApplicationServices_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostApplicationBuilderMock = new Mock<IHostApplicationBuilder>();
        hostApplicationBuilderMock.Setup(b => b.Services).Returns(services);

        // Act
        hostApplicationBuilderMock.Object.AddApplicationServices();

        // Assert
        services.ShouldNotBeNull();
        // Add more specific assertions for services you expect to be registered
        // For example, check for MediatR, AutoMapper, FluentValidation
        services.ShouldContain(s => s.ServiceType == typeof(MediatR.IMediator));
        services.ShouldContain(s => s.ServiceType == typeof(AutoMapper.IMapper));
    }
}