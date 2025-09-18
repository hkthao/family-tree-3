using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly GetMembersQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersQueryHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);

        // Setup AutoMapper
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMembersQueryHandler(_context, _mapper);
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
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new MemberDto { Id = "1", FullName = "Member 1" });
        result.Should().ContainEquivalentOf(new MemberDto { Id = "2", FullName = "Member 2" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoMembersExist()
    {
        // Arrange - no members added to context

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}