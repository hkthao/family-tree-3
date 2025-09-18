using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetMembersQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Members()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { Id = "1", FullName = "Member 1" },
            new Member { Id = "2", FullName = "Member 2" }
        };
        var memberDtos = new List<MemberDto>
        {
            new MemberDto { Id = "1", FullName = "Member 1" },
            new MemberDto { Id = "2", FullName = "Member 2" }
        };

        var cursor = new Mock<IAsyncCursor<Member>>();
        cursor.Setup(_ => _.Current).Returns(members);
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));

        var collectionMock = new Mock<IMongoCollection<Member>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<FindOptions<Member, Member>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);
        
        _contextMock.Setup(x => x.Members).Returns(collectionMock.Object);
        _mapperMock.Setup(m => m.Map<List<MemberDto>>(It.IsAny<List<Member>>())).Returns(memberDtos);

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(memberDtos);
        _contextMock.Verify(x => x.Members, Times.Once);
        collectionMock.Verify(x => x.FindAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<FindOptions<Member, Member>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<MemberDto>>(It.IsAny<List<Member>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Arrange
        var members = new List<Member>();
        var memberDtos = new List<MemberDto>();

        var cursor = new Mock<IAsyncCursor<Member>>();
        cursor.Setup(_ => _.Current).Returns(members);
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(false));

        var collectionMock = new Mock<IMongoCollection<Member>>();
        collectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<FindOptions<Member, Member>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);
        
        _contextMock.Setup(x => x.Members).Returns(collectionMock.Object);
        _mapperMock.Setup(m => m.Map<List<MemberDto>>(It.IsAny<List<Member>>())).Returns(memberDtos);

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _contextMock.Verify(x => x.Members, Times.Once);
        collectionMock.Verify(x => x.FindAsync(
            It.IsAny<FilterDefinition<Member>>(),
            It.IsAny<FindOptions<Member, Member>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<MemberDto>>(It.IsAny<List<Member>>()), Times.Once);
    }
}
