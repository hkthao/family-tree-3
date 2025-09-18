using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Mappings;
using backend.Application.Families.Queries.GetFamilies;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries;

public class GetFamiliesQueryHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family { Id = Guid.NewGuid(), Name = "Family 1" },
            new Family { Id = Guid.NewGuid(), Name = "Family 2" }
        };
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetFamiliesQueryHandler(_context, _mapper);

        // Act
        var result = await handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new { Name = "Family 1" });
        result.Should().ContainEquivalentOf(new { Name = "Family 2" });
    }
}
