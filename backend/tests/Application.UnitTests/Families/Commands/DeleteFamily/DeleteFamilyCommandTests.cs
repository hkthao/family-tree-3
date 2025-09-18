using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MongoDB.Driver;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandTests
{
    [Fact]
    public async Task ShouldDeleteFamily()
    {
        var familyId = MongoDB.Bson.ObjectId.GenerateNewId();
        var command = new DeleteFamilyCommand(familyId.ToString());

        var mockFamilyCollection = new Mock<IMongoCollection<Family>>();
        mockFamilyCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Family>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(1));

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Families).Returns(mockFamilyCollection.Object);

        var handler = new DeleteFamilyCommandHandler(mockContext.Object);

        await handler.Handle(command, CancellationToken.None);
    }
}
