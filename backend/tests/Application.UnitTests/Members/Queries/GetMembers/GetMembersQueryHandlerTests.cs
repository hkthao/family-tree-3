using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMembers;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

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
            new Member { Id = Guid.NewGuid(), FullName = "Member 1", FamilyId = Guid.NewGuid() },
            new Member { Id = Guid.NewGuid(), FullName = "Member 2", FamilyId = Guid.NewGuid() }
        };
        _context.Members.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMembersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new MemberDto { Id = members[0].Id, FullName = "Member 1" });
        result.Should().ContainEquivalentOf(new MemberDto { Id = members[1].Id, FullName = "Member 2" });
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
