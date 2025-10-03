using AutoMapper;
using backend.Application.Members.Queries.SearchMembers;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Infrastructure.Data;
using backend.Application.Common.Mappings;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Members.Queries.SearchMembers;

public class SearchMembersQueryHandlerTests : IDisposable
{
    private readonly SearchMembersQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchMembersQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new SearchMembersQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Matching_Members()
    {
        // Arrange
        _context.Members.AddRange(
            new Member { FirstName = "John", LastName = "Doe", Nickname = "Johnny" },
            new Member { FirstName = "Jane", LastName = "Doe", Nickname = "Janey" },
            new Member { FirstName = "Peter", LastName = "Jones", Nickname = "Pete" }
        );
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { Keyword = "Doe" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.FirstName == "John");
        result.Items.Should().Contain(x => x.FirstName == "Jane");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
