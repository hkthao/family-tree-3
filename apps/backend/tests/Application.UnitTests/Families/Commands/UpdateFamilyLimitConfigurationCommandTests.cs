using backend.Application.Families.Commands;
using backend.Domain.Entities;
using FluentAssertions; // FluentAssertions
using Xunit;
using backend.Application.Common.Models;
using backend.Infrastructure.Data;
using backend.Application.UnitTests.Common; // Corrected using statement for TestBase
using Microsoft.EntityFrameworkCore; // For Include
using System; // For Action

namespace backend.Tests.Application.UnitTests.Families.Commands;

public class UpdateFamilyLimitConfigurationCommandTests : TestBase
{
    private readonly UpdateFamilyLimitConfigurationCommandHandler _handler;

    public UpdateFamilyLimitConfigurationCommandTests()
    {
        _handler = new UpdateFamilyLimitConfigurationCommandHandler(_context); // Use inherited _context
    }

    [Fact]
    public async Task Handle_GivenValidCommand_ShouldUpdateFamilyLimitConfiguration()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF001", null, null, "Private", Guid.NewGuid());
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = family.Id,
            MaxMembers = 100,
            MaxStorageMb = 2048
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamily = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == family.Id);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyLimitConfiguration.Should().NotBeNull();
        updatedFamily.FamilyLimitConfiguration!.MaxMembers.Should().Be(command.MaxMembers);
        updatedFamily.FamilyLimitConfiguration!.MaxStorageMb.Should().Be(command.MaxStorageMb);
    }

    [Fact]
    public async Task Handle_GivenNonExistentFamilyId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = Guid.NewGuid(), // Non-existent ID
            MaxMembers = 100,
            MaxStorageMb = 2048
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Không tìm thấy gia đình với ID '{command.FamilyId}'.");
    }

    [Fact]
    public async Task Handle_GivenInvalidMaxMembers_ShouldThrowArgumentException()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF001", null, null, "Private", Guid.NewGuid());
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var command = new UpdateFamilyLimitConfigurationCommand
        {
            FamilyId = family.Id,
            MaxMembers = 0, // Invalid value
            MaxStorageMb = 2048
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
