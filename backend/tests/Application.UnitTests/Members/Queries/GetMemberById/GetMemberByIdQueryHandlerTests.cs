using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Members.Queries.GetMemberById;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Identity.UserProfiles.Queries; // Added for MappingProfile

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : IDisposable
{
    private readonly GetMemberByIdQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMemberByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Member_When_Found()
    {
        // Arrange
        var memberId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Use a seeded member ID
        var member = await _context.Members.FindAsync(memberId);

        // Act
        var result = await _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
        result.Value.Should().Be(member!.FirstName);
        result.Value.Should().Be(member.LastName);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var command = new GetMemberByIdQuery(Guid.NewGuid());

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
