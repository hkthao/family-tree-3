using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests
{
    private readonly GetFamilyByIdQueryHandler _handler;
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandlerTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();

        // Setup AutoMapper
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetFamilyByIdQueryHandler(_mockFamilyRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Family_When_Found()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Family 1" };
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(familyId)).ReturnsAsync(family);

        // Act
        var result = await _handler.Handle(new GetFamilyByIdQuery(familyId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Name.Should().Be(family.Name);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var command = new GetFamilyByIdQuery(Guid.NewGuid());
        _mockFamilyRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((Family?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
