using Xunit;
using Moq;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.CreateFamily;
using System.Threading;
using System.Threading.Tasks;
using backend.Domain.Entities;
using FluentAssertions;

namespace backend.tests.Application.UnitTests.Families;

public class FamilyServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;

    public FamilyServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Fact]
    public async Task CreateFamily_ShouldCreateFamily_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateFamilyCommand { Name = "Test Family" };
        var handler = new CreateFamilyCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.Families.InsertOneAsync(It.IsAny<Family>(), null, CancellationToken.None), Times.Once);
        result.Should().NotBeNullOrEmpty();
    }
}
