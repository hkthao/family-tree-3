using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests : IDisposable
{
    private readonly DeleteFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public DeleteFamilyCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new DeleteFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var command = new DeleteFamilyCommand(familyId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedFamily = await _context.Families.FindAsync(familyId);
        deletedFamily.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new DeleteFamilyCommand(invalidId);
        var handler = new DeleteFamilyCommandHandler(_context);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
