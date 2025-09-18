using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.FamilyTree.Queries.GetFamilyTreeJson;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace backend.Application.UnitTests.FamilyTree.Queries.GetFamilyTreeJson;

public class GetFamilyTreeJsonQueryHandlerTests
{
    private readonly GetFamilyTreeJsonQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyTreeJsonQueryHandlerTests()
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

        _handler = new GetFamilyTreeJsonQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamilyTreeDto_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        var member = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FullName = "Test Member" };
        var relationship = new Relationship { Id = Guid.NewGuid(), FamilyId = familyId, SourceMemberId = Guid.NewGuid(), TargetMemberId = Guid.NewGuid(), Type = Domain.Enums.RelationshipType.Parent };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var query = new GetFamilyTreeJsonQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Family.Should().NotBeNull();
        result.Family!.Id.Should().Be(familyId);
        result.Members.Should().ContainSingle(m => m.Id == member.Id);
        result.Relationships.Should().ContainSingle(r => r.Id == relationship.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDoesNotExist()
    {
        // Arrange
        var query = new GetFamilyTreeJsonQuery(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
