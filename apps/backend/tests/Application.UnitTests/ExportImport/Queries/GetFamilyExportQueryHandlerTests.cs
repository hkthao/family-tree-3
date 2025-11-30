using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.ExportImport.Commands;
using backend.Application.ExportImport.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.ExportImport.Queries;

public class GetFamilyExportQueryHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GetFamilyExportQueryHandler _handler;

    public GetFamilyExportQueryHandlerTests() : base()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        _handler = new GetFamilyExportQueryHandler(_context, _mapper, _authorizationServiceMock.Object);
    }



    [Fact]
    public async Task Handle_ShouldReturnFamilyExportDto_WhenAuthorizedAndFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetFamilyExportQuery(familyId);
        var familyEntity = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF1"
        };

        var familyExportDto = new FamilyExportDto
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF1",
            Visibility = "Private"
        };

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        _context.Families.Add(familyEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(familyExportDto);
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetFamilyExportQuery(familyId);
        var familyEntity = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        _context.Families.Add(familyEntity);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var query = new GetFamilyExportQuery(familyId);

        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, familyId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        _authorizationServiceMock.Verify(x => x.CanAccessFamily(familyId), Times.Once);
    }
}
