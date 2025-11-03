using backend.Application.Members.Queries.GetMemberById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    public GetMemberByIdQueryHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member("John", "Doe", "JD", Guid.NewGuid()) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var handler = new GetMemberByIdQueryHandler(_context, _mapper);
        var query = new GetMemberByIdQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberDoesNotExist()
    {
        // Arrange
        var handler = new GetMemberByIdQueryHandler(_context, _mapper);
        var query = new GetMemberByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
