using backend.Application.Families;
using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamilyByIdQueryHandler _handler;

    public GetFamilyByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFamilyByIdQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Family_When_Found()
    {
        // Arrange
        var family = new Family { Name = "Family 1" };
        var familyDto = new FamilyDto { Name = "Family 1" };

        var cursor = new Mock<IAsyncCursor<Family>>();
        cursor.Setup(_ => _.Current).Returns(new List<Family> { family });
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
        _mapperMock.Setup(m => m.Map<FamilyDto>(It.IsAny<Family>())).Returns(familyDto);

        // Act
        var result = await _handler.Handle(new GetFamilyByIdQuery("1"), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(familyDto);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var cursor = new Mock<IAsyncCursor<Family>>();
        cursor.Setup(_ => _.Current).Returns(new List<Family>());
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(false));
        
        var collectionMock = new Mock<IMongoCollection<Family>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Family>>(),
            It.IsAny<FindOptions<Family, Family>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        _contextMock.Setup(x => x.Families).Returns(collectionMock.Object);

        // Act
        var act = () => _handler.Handle(new GetFamilyByIdQuery("1"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
