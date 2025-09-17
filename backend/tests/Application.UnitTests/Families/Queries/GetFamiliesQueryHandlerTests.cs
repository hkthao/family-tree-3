using backend.Application.Families;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilies;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries;

public class GetFamiliesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamiliesQueryHandler _handler;

    public GetFamiliesQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFamiliesQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family { Name = "Family 1" },
            new Family { Name = "Family 2" }
        };
        var familyDtos = new List<FamilyDto>
        {
            new FamilyDto { Name = "Family 1" },
            new FamilyDto { Name = "Family 2" }
        };

        var cursor = new Mock<IAsyncCursor<Family>>();
        cursor.Setup(_ => _.Current).Returns(families);
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));

        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<FindOptions<Family, Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);
        _mapperMock.Setup(m => m.Map<List<FamilyDto>>(It.IsAny<List<Family>>())).Returns(familyDtos);

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(familyDtos);
    }
}
