using backend.Application.Common.Exceptions;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandlerTests : IDisposable
{
    private readonly UpdateFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public UpdateFamilyCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UpdateFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Update_Family()
    {
        // Arrange
        var familyId = Guid.NewGuid();
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
        var command = new UpdateFamilyCommand { Id = Guid.NewGuid() };

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
