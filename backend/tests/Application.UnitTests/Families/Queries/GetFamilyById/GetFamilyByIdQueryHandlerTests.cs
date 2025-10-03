using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.Families.Queries.GetFamilyById;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests : IDisposable
{
    private readonly GetFamilyByIdQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetFamilyByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Family_When_Found()
    {
        // Arrange
        var familyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Use a seeded family ID
        var family = await _context.Families.FindAsync(familyId);

        // Act
        var result = await _handler.Handle(new GetFamilyByIdQuery(familyId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Name.Should().Be(family!.Name);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var command = new GetFamilyByIdQuery(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
