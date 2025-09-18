using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Infrastructure.Data;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests
{
    private readonly GetMemberByIdQueryHandler _handler;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);

        // Setup AutoMapper
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();

        _handler = new GetMemberByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_Member_When_Found()
    {
        // Arrange
        var memberId = "65e6f8a2b3c4d5e6f7a8b9c0";
        var member = new Member { Id = memberId, FullName = "Test Member" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(memberId);
        result.FullName.Should().Be(member.FullName);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var command = new GetMemberByIdQuery("000000000000000000000000"); // Valid format, but non-existent

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}