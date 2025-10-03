using AutoMapper;
using backend.Application.Common.Mappings;
using backend.Application.Families.Queries.GetFamilies;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Families.Queries;

public class GetFamiliesQueryHandlerTests
{
    private readonly Mock<IFamilyRepository> _mockFamilyRepository;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandlerTests()
    {
        _mockFamilyRepository = new Mock<IFamilyRepository>();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family { Id = Guid.NewGuid(), Name = "Family 1" },
            new Family { Id = Guid.NewGuid(), Name = "Family 2" }
        };
        _mockFamilyRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(families);

        var handler = new GetFamiliesQueryHandler(_mockFamilyRepository.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new { Name = "Family 1" });
        result.Should().ContainEquivalentOf(new { Name = "Family 2" });
    }
}
