using backend.Application.Common.Interfaces;
using backend.Application.FamilyTree.Queries.GetFamilyTreeJson;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MongoDB.Driver;

namespace backend.Application.UnitTests.FamilyTree.Queries.GetFamilyTreeJson;

public class GetFamilyTreeJsonQueryTests
{
    [Fact]
    public async Task ShouldReturnFamilyTreeJson()
    {
        var query = new GetFamilyTreeJsonQuery(MongoDB.Bson.ObjectId.GenerateNewId().ToString());

        var mockFamilyCursor = new Mock<IAsyncCursor<Family>>();
        mockFamilyCursor.Setup(c => c.Current).Returns(new List<Family> { new Family() });
        mockFamilyCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockFamilyCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));

        var mockFamilyCollection = new Mock<IMongoCollection<Family>>();
        mockFamilyCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Family>>(), It.IsAny<FindOptions<Family, Family>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFamilyCursor.Object);

        var mockMemberCursor = new Mock<IAsyncCursor<Member>>();
        mockMemberCursor.Setup(c => c.Current).Returns(new List<Member> { new Member() });
        mockMemberCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockMemberCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));

        var mockMemberCollection = new Mock<IMongoCollection<Member>>();
        mockMemberCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Member>>(), It.IsAny<FindOptions<Member, Member>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMemberCursor.Object);

        var mockRelationshipCursor = new Mock<IAsyncCursor<Relationship>>();
        mockRelationshipCursor.Setup(c => c.Current).Returns(new List<Relationship> { new Relationship() });
        mockRelationshipCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockRelationshipCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));

        var mockRelationshipCollection = new Mock<IMongoCollection<Relationship>>();
        mockRelationshipCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Relationship>>(), It.IsAny<FindOptions<Relationship, Relationship>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRelationshipCursor.Object);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Families).Returns(mockFamilyCollection.Object);
        mockContext.Setup(c => c.Members).Returns(mockMemberCollection.Object);
        mockContext.Setup(c => c.Relationships).Returns(mockRelationshipCollection.Object);

        var mockMapper = new Mock<AutoMapper.IMapper>();

        var handler = new GetFamilyTreeJsonQueryHandler(mockContext.Object, mockMapper.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
    }
}
