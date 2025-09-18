
using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Mappings;
using backend.Application.Members;
using backend.Application.Members.Queries.GetMemberById;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Members;

public class MemberServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MemberServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configurationProvider.CreateMapper();
    }

    [Fact]
    public async Task GetMemberById_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FullName = "John Doe", FamilyId = Guid.NewGuid() };
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMemberByIdQueryHandler(_context, _mapper);
        var query = new GetMemberByIdQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(memberId);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetMemberById_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        // Arrange
        var query = new GetMemberByIdQuery(Guid.NewGuid());
        var handler = new GetMemberByIdQueryHandler(_context, _mapper);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
