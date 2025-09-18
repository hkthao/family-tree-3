
using backend.Application.Families;
using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;
using backend.Application.Common.Mappings;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests
{
    private readonly GetFamilyByIdQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandlerTests()
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

        _handler = new GetFamilyByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Family_When_Found()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Family 1" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetFamilyByIdQuery(familyId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Name.Should().Be(family.Name);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var command = new GetFamilyByIdQuery(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
