using backend.Application.Families.Queries.GetFamilies;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : TestBase
{
    private readonly GetFamiliesQueryHandler _handler;

    public GetFamiliesQueryHandlerTests()
    {
        _handler = new GetFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Data is seeded by TestDbContextFactory

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(1); // Royal Family is seeded
        result.Value.Should().ContainEquivalentOf(new { Name = "Royal Family" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoFamiliesExist()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var emptyContext = TestDbContextFactory.Create(false);
        var emptyHandler = new GetFamiliesQueryHandler(emptyContext, _mapper, _mockUser.Object, _mockAuthorizationService.Object);

        // Act
        var result = await emptyHandler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();

        TestDbContextFactory.Destroy(emptyContext);
    }
}
