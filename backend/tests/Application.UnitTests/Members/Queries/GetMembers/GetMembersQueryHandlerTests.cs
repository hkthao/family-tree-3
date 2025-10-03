using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly IMapper _mapper;
    private readonly GetMembersQueryHandler _handler;
    private readonly Mock<DbSet<Member>> _mockDbSetMember;

    public GetMembersQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDbSetMember = new Mock<DbSet<Member>>();

        _mockContext.Setup(c => c.Members).Returns(_mockDbSetMember.Object);

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMembersQueryHandler(_mockContext.Object, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Members()
    {
        // Arrange
        var members = new List<Member>
        {
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow }
        };

        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(members.AsQueryable().Provider);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(members.AsQueryable().Expression);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(members.AsQueryable().ElementType);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(members.AsQueryable().GetEnumerator());

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new { FullName = "1 Member" });
        result.Should().ContainEquivalentOf(new { FullName = "2 Member" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Arrange
        var members = new List<Member>();

        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(members.AsQueryable().Provider);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(members.AsQueryable().Expression);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(members.AsQueryable().ElementType);
        _mockDbSetMember.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(members.AsQueryable().GetEnumerator());

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
