using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.UpdateMember;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Members.Inputs;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : IDisposable
{
    private readonly UpdateMemberCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public UpdateMemberCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = Guid.NewGuid() };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "Updated",
            LastName = "Name",
            Gender = "Female",
            Relationships = new List<RelationshipInput>(),
            DeletedRelationshipIds = new List<Guid>()
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedMember = await _context.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be("Updated");
        updatedMember.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = invalidId, Relationships = new List<RelationshipInput>(), DeletedRelationshipIds = new List<Guid>() };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}