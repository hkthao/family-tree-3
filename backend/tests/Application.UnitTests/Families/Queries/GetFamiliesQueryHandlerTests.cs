using AutoMapper;
using backend.Application.Families.Queries.GetFamilies;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Common.Mappings;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : IDisposable
{
    private readonly GetFamiliesQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamiliesQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetFamiliesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Families()
    {
        // Arrange
        // Data is seeded by TestDbContextFactory

        // Act
        var result = await _handler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1); // Royal Family is seeded
        result.Should().ContainEquivalentOf(new { Name = "Royal Family" });
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoFamiliesExist()
    {
        // Arrange
        var emptyContext = TestDbContextFactory.Create();
        var emptyHandler = new GetFamiliesQueryHandler(emptyContext, _mapper);

        // Act
        var result = await emptyHandler.Handle(new GetFamiliesQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        TestDbContextFactory.Destroy(emptyContext);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
