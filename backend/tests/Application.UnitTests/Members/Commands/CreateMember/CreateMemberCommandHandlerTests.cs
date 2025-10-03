using backend.Application.Members.Commands.CreateMember;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Members.Inputs;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : IDisposable
{
    private readonly CreateMemberCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public CreateMemberCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnId()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Member",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            FamilyId = Guid.NewGuid(),
            Relationships = new List<RelationshipInput>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        var createdMember = await _context.Members.FindAsync(result);
        createdMember.Should().NotBeNull();
        createdMember!.FirstName.Should().Be("Test");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}