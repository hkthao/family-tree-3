using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.FamilyTree.Queries.GetFamilyTreePdf;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.FamilyTree.Queries;

public class GetFamilyTreePdfQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetFamilyTreePdfQueryHandler _handler;
    private readonly Mock<IMongoCollection<Family>> _mockFamiliesCollection;

    public GetFamilyTreePdfQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockFamiliesCollection = new Mock<IMongoCollection<Family>>();
        _mockContext.Setup(c => c.Families).Returns(_mockFamiliesCollection.Object);
        _handler = new GetFamilyTreePdfQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfContent_WhenFamilyExists()
    {
        // Arrange
        var familyId = "60c72b2f9b1e8b001c8e4e1a";
        var family = new Family { Id = familyId, Name = "Test Family" };

        var mockAsyncCursor = new Mock<IAsyncCursor<Family>>();
        mockAsyncCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockAsyncCursor.SetupGet(_ => _.Current).Returns(new[] { family });

        var mockFindFluent = new Mock<IFindFluent<Family, Family>>();
        mockFindFluent.Setup(_ => _.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockAsyncCursor.Object);

        _mockFamiliesCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Family>>(), It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var query = new GetFamilyTreePdfQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        System.Text.Encoding.UTF8.GetString(result).Should().Contain($"Dummy PDF content for Family Tree of {family.Name} (ID: {family.Id})");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDoesNotExist()
    {
        // Arrange
        var familyId = "60c72b2f9b1e8b001c8e4e1a";

        var mockAsyncCursor = new Mock<IAsyncCursor<Family>>();
        mockAsyncCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockAsyncCursor.SetupGet(_ => _.Current).Returns(new Family[0]);

        var mockFindFluent = new Mock<IFindFluent<Family, Family>>();
        mockFindFluent.Setup(_ => _.ToCursorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockAsyncCursor.Object);

        _mockFamiliesCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Family>>(), It.IsAny<FindOptions>()))
            .Returns(mockFindFluent.Object);

        var query = new GetFamilyTreePdfQuery(familyId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Entity \"Family\" ({familyId}) was not found.");
    }
}