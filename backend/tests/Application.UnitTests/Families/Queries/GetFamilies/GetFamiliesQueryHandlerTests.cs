using backend.Application.Families;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilies;
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

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests
{
    private readonly GetFamiliesQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandlerTests()
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

        _handler = new GetFamiliesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family { Id = "1", Name = "Family 1" },
            new Family { Id = "2", Name = "Family 2" }
        };
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new FamilyDto { Id = "1", Name = "Family 1" });
        result.Should().ContainEquivalentOf(new FamilyDto { Id = "2", Name = "Family 2" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoFamiliesExist()
    {
        // Arrange - no families added to context

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}