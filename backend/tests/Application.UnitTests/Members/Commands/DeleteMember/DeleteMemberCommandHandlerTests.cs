
using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.DeleteMember;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public DeleteMemberCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = Guid.NewGuid() };
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new DeleteMemberCommand(memberId);
        var handler = new DeleteMemberCommandHandler(_context);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedMember = await _context.Members.FindAsync(memberId);
        deletedMember.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new DeleteMemberCommand(invalidId);
        var handler = new DeleteMemberCommandHandler(_context);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
