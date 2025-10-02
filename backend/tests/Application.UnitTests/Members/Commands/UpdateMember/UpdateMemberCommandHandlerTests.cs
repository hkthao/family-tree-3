
using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.UpdateMember;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public UpdateMemberCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = Guid.NewGuid() };
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "Updated",
            LastName = "Name",
            Gender = "Female"
        };
        var handler = new UpdateMemberCommandHandler(_context);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedMember = await _context.Members.FindAsync(memberId);
        updatedMember.Should().NotBeNull();
        updatedMember?.FirstName.Should().Be("Updated");
        updatedMember?.LastName.Should().Be("Name");
        updatedMember?.Gender.Should().Be("Female");
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var command = new UpdateMemberCommand { Id = invalidId };
        var handler = new UpdateMemberCommandHandler(_context);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
