
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Application.Families.Queries.GetFamilies;
using backend.Domain.Entities;

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

        var families = new List<Family>
        {
            new Family { Id = Guid.NewGuid(), Name = "Family 1" },
            new Family { Id = Guid.NewGuid(), Name = "Family 2" }
        };
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
        result.Value.Should().ContainEquivalentOf(new { Id = families[0].Id, Name = "Family 1" });
        result.Value.Should().ContainEquivalentOf(new { Id = families[1].Id, Name = "Family 2" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoFamiliesExist()
    {
        // Arrange
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
    }
}
