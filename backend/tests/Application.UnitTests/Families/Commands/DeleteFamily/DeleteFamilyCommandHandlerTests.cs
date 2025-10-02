using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.DeleteFamily;

public class DeleteFamilyCommandHandlerTests
{
    private readonly DeleteFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public DeleteFamilyCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new DeleteFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Delete_Family()
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
        _context.Families.Should().NotContain(f => f.Id == familyId);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new DeleteFamilyCommand(Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
