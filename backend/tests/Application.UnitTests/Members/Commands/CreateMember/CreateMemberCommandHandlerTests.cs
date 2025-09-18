using backend.Application.Members.Commands.CreateMember;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public CreateMemberCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnId()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FullName = "Test Member",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            FamilyId = Guid.NewGuid()
        };

        var handler = new CreateMemberCommandHandler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdMember = await _context.Members.FindAsync(result);
        createdMember.Should().NotBeNull();
        createdMember!.FullName.Should().Be("Test Member");
    }
}