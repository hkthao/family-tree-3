using backend.Application.Common.Constants;
using backend.Application.Families.Queries.GetPublicFamilyById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetPublicFamilyById;

public class GetPublicFamilyByIdQueryHandlerTests : TestBase
{
    private readonly GetPublicFamilyByIdQueryHandler _handler;

    public GetPublicFamilyByIdQueryHandlerTests()
    {
        _handler = new GetPublicFamilyByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPublicFamily_WhenFamilyExistsAndIsPublic()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var testFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(testFamily);
        await _context.SaveChangesAsync();

        var query = new GetPublicFamilyByIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(familyId);
        result.Value.Name.Should().Be(testFamily.Name);
        result.Value.Visibility.Should().Be(FamilyVisibility.Public.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var query = new GetPublicFamilyByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, query.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyExistsButIsPrivate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var testFamily = new Family { Id = familyId, Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.Add(testFamily);
        await _context.SaveChangesAsync();

        var query = new GetPublicFamilyByIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
