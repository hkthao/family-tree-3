using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests
{
    private readonly UpdateFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public UpdateFamilyCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new UpdateFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Update_Family()
    {
        // Arrange
        var familyId = "60c72b2f9b1e8b001c8e4e1a";
        var family = new Family { Id = familyId, Name = "Old Name" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyCommand
        {
            Id = familyId,
            Name = "New Name",
            Description = "New Desc"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedFamily = await _context.Families.FindAsync(familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.Name.Should().Be(command.Name);
        updatedFamily.Description.Should().Be(command.Description);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Family_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateFamilyCommand { Id = "1" };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}