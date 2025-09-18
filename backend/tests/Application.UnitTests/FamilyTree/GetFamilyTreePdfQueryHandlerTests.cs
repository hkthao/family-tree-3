using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Common.Interfaces;
using backend.Application.FamilyTree.Queries.GetFamilyTreePdf;
using backend.Domain.Entities;
using backend.Infrastructure.Data; // Added for ApplicationDbContext
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.FamilyTree;

public class GetFamilyTreePdfQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetFamilyTreePdfQueryHandler _handler;

    public GetFamilyTreePdfQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test run
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Ensure the in-memory database is created

        _handler = new GetFamilyTreePdfQueryHandler(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // Clean up the in-memory database after each test
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfContent_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new GetFamilyTreePdfQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        System.Text.Encoding.UTF8.GetString(result).Should().Contain($"Dummy PDF content for Family Tree of {family.Name} (ID: {family.Id})");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFamilyDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Ensure this ID does not exist in the database

        var query = new GetFamilyTreePdfQuery(familyId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<backend.Application.Common.Exceptions.NotFoundException>()
            .WithMessage($"Entity \"Family\" ({familyId}) was not found.");
    }
}
