﻿using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;
using MongoDB.Driver;
using Moq;
using Xunit;
using FluentAssertions;
using backend.Application.Common.Exceptions;
using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Members;

namespace backend.Application.UnitTests.Members;

public class MemberServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMongoCollection<Member>> _mockCollection;
    private readonly IMapper _mapper;

    public MemberServiceTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCollection = new Mock<IMongoCollection<Member>>();

        // Setup the mock context to return the mock collection
        _mockContext.Setup(c => c.Members).Returns(_mockCollection.Object);

        // Configure AutoMapper
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });
        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public async Task GetMemberById_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var memberId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var expectedMember = new Member
        {
            Id = memberId,
            FullName = "John Doe",
            Gender = "Male",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        var mockCursor = new Mock<IAsyncCursor<Member>>();
        mockCursor.Setup(_ => _.Current).Returns(new[] { expectedMember });
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        // This is the corrected setup for FindAsync
        _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Member>>(),
                It.IsAny<FindOptions<Member, Member>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var handler = new GetMemberByIdQueryHandler(_mockContext.Object, _mapper);
        var query = new GetMemberByIdQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<MemberDto>();
        result.Id.Should().Be(memberId);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetMemberById_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = "000000000000000000000000";

        // Setup the mock cursor to return no results
        var mockCursor = new Mock<IAsyncCursor<Member>>();
        mockCursor.Setup(_ => _.Current).Returns(new List<Member>());
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Correct setup for FindAsync to return an empty cursor
        _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Member>>(),
                It.IsAny<FindOptions<Member, Member>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var handler = new GetMemberByIdQueryHandler(_mockContext.Object, _mapper);
        var query = new GetMemberByIdQuery(memberId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}