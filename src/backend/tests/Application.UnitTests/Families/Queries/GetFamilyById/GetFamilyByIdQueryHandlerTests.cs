using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetFamilyByIdQueryHandler _handler;

    public GetFamilyByIdQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _handler = new GetFamilyByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFamily_WhenFamilyExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var testFamily = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        _context.Families.Add(testFamily);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(familyId);

        // Setup mapper to return FamilyDetailDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDetailDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(familyId);
        result.Value.Name.Should().Be(testFamily.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var query = new GetFamilyByIdQuery(Guid.NewGuid());

        // Setup mapper to return FamilyDetailDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyDetailDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, query.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
