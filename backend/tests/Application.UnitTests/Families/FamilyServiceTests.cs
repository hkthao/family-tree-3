using backend.Application.Families.Commands.CreateFamily;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Families;

public class FamilyServiceTests
{
    private readonly ApplicationDbContext _context;

    public FamilyServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateFamily_ShouldCreateFamily_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateFamilyCommand { Name = "Test Family" };
        var handler = new CreateFamilyCommandHandler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdFamily = await _context.Families.FindAsync(result);
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be("Test Family");
    }
}