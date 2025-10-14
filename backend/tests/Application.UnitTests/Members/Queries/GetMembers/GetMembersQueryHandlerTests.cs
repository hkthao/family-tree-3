using AutoMapper;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();
        _handler = new GetMembersQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Members()
    {
        // Arrange
        int countBefore = _context.Members.Count();
        var members = new List<Member>
        {
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = Guid.NewGuid(), Created = DateTime.UtcNow }
        };
        _context.AddRange(members);
        _context.SaveChanges();

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);
        // Assert
        result.Should().HaveCount(countBefore + 2);
        result.Should().ContainEquivalentOf(new { FullName = "1 Member" });
        result.Should().ContainEquivalentOf(new { FullName = "2 Member" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Act
        var result = await _handler.Handle(new GetMembersQuery() { FamilyId = Guid.NewGuid() }, CancellationToken.None);
        // Assert
        result.Should().BeEmpty();
    }
}
