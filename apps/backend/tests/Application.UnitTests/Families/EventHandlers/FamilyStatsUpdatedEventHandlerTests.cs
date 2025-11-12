using backend.Application.Common.Interfaces;
using backend.Application.Families.EventHandlers;
using backend.Application.UnitTests.Common;
using backend.Domain.Events.Families;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.EventHandlers;

public class FamilyStatsUpdatedEventHandlerTests : TestBase
{
    private readonly Mock<IFamilyTreeService> _familyTreeServiceMock;
    private readonly FamilyStatsUpdatedEventHandler _handler;

    public FamilyStatsUpdatedEventHandlerTests()
    {
        _familyTreeServiceMock = new Mock<IFamilyTreeService>();
        _handler = new FamilyStatsUpdatedEventHandler(_familyTreeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateFamilyStats()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var notification = new FamilyStatsUpdatedEvent(familyId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _familyTreeServiceMock.Verify(s => s.UpdateFamilyStats(familyId, CancellationToken.None), Times.Once);
    }
}
