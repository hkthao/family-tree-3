using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.FamilyTree.Queries.GetFamilyTreeJson;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.FamilyTree.Queries.GetFamilyTreeJson;

public class GetFamilyTreeJsonQueryTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyTreeJsonQueryTests()
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
    public async Task Handle_ShouldReturnFamilyTreeJson()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        var member = new Member { FamilyId = familyId, FullName = "Test Member" }; // Corrected FamilyId type
        var anotherMember = new Member { FamilyId = familyId, FullName = "Another Member" }; // Create another member for relationship
        var relationship = new Relationship
        {
            FamilyId = familyId,
            SourceMemberId = member.Id,
            TargetMemberId = anotherMember.Id,
            Type = backend.Domain.Enums.RelationshipType.Parent,
            StartDate = DateTime.UtcNow
        };

        _context.Families.Add(family);
        _context.Members.Add(member);
        _context.Members.Add(anotherMember); // Add the new member
        _context.Relationships.Add(relationship); // Add the relationship
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFamilyTreeJsonQuery(familyId);
        var handler = new GetFamilyTreeJsonQueryHandler(_context, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Family!.Id.Should().Be(familyId);
        result.Members.Should().HaveCount(2); // Now two members
        result.Members.First().FullName.Should().Be("Test Member");
        result.Relationships.First().SourceMemberId.Should().Be(member.Id);
        result.Relationships.First().TargetMemberId.Should().Be(anotherMember.Id);
    }
}
