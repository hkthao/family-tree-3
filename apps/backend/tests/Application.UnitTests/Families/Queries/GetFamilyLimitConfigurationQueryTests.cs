using System; // For Guid.Empty
using AutoMapper;
using backend.Application.Families.DTOs; // For FamilyLimitConfigurationMapperProfile
using backend.Application.Families.Queries;
using backend.Application.UnitTests.Common; // Corrected using statement for TestBase
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions; // FluentAssertions
using Xunit;

namespace backend.Tests.Application.UnitTests.Families.Queries;

public class GetFamilyLimitConfigurationQueryTests : TestBase
{
    private readonly GetFamilyLimitConfigurationQueryHandler _handler;

    public GetFamilyLimitConfigurationQueryTests()
    {
        _handler = new GetFamilyLimitConfigurationQueryHandler(_context, _mapper); // Use inherited _context and _mapper
    }

    [Fact]
    public async Task Handle_GivenExistingFamilyWithConfiguration_ShouldReturnFamilyLimitConfigurationDto()
    {
        // Arrange
        var family = Family.Create("Test Family", "TF001", null, null, "Private", Guid.NewGuid());
        family.UpdateFamilyConfiguration(100, 2048, 500); // Ensure it has a custom config with AI Chat Limit
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new GetFamilyLimitConfigurationQuery { FamilyId = family.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(family.Id);
        result.Value!.MaxMembers.Should().Be(100);
        result.Value!.MaxStorageMb.Should().Be(2048);
        result.Value!.AiChatMonthlyLimit.Should().Be(500); // New assertion
    }

    [Fact]
    public async Task Handle_GivenFamilyWithoutSpecificConfiguration_ShouldReturnDefaultFamilyLimitConfigurationDto()
    {
        // Arrange
        var family = Family.Create("Test Family No Config", "TF002", null, null, "Private", Guid.NewGuid());
        // Do not call UpdateFamilyConfiguration, rely on default initialization
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new GetFamilyLimitConfigurationQuery { FamilyId = family.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(family.Id);
        result.Value!.MaxMembers.Should().Be(50); // Default value
        result.Value!.MaxStorageMb.Should().Be(1024); // Default value
    }

    [Fact]
    public async Task Handle_GivenNonExistentFamilyId_ShouldReturnDefaultFamilyLimitConfigurationDto()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new GetFamilyLimitConfigurationQuery { FamilyId = nonExistentFamilyId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Should().NotBeNull();
        result.Value!.FamilyId.Should().Be(nonExistentFamilyId);
        result.Value!.MaxMembers.Should().Be(50); // Default value as per handler's logic
        result.Value!.MaxStorageMb.Should().Be(1024); // Default value as per handler's logic
        result.Value!.Id.Should().Be(Guid.Empty); // ID should be Guid.Empty for non-existent config
    }
}
