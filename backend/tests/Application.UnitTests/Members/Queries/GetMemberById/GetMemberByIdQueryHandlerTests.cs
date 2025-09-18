using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetMemberByIdQueryHandler _handler;

    public GetMemberByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetMemberByIdQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Member_When_Found()
    {
        // Arrange
        var memberId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var member = new Member { Id = memberId, FullName = "Test Member" };
        var memberDto = new MemberDto { Id = memberId, FullName = "Test Member" };

        var cursor = new Mock<IAsyncCursor<Member>>();
        cursor.Setup(_ => _.Current).Returns(new List<Member> { member });
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
        _mapperMock.Setup(m => m.Map<MemberDto>(It.IsAny<Member>())).Returns(memberDto);

        // Act
        var result = await _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(memberDto);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var memberId = "000000000000000000000000"; // Valid format, but non-existent

        var cursor = new Mock<IAsyncCursor<Member>>();
        cursor.Setup(_ => _.Current).Returns(new List<Member>());
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

        // Act
        var act = () => _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
