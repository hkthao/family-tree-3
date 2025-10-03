using backend.Application.Families.Commands.CreateFamily;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : IDisposable
{
    private readonly CreateFamilyCommandHandler _handler;
    private readonly ApplicationDbContext _context;

    public CreateFamilyCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Create_Family_And_Return_Id()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "New Test Family",
            AvatarUrl = "new_logo.png",
            Description = "A new family..."
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        var createdFamily = await _context.Families.FindAsync(result);
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be(command.Name);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
