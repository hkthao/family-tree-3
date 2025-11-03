using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GetFamiliesQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách tất cả các gia đình khi người dùng là admin.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllFamilies_WhenUserIsAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "FAM001", TotalMembers = 10, TotalGenerations = 3 };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "FAM002", TotalMembers = 5, TotalGenerations = 2 };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "FAM003", TotalMembers = 7, TotalGenerations = 4 };

        _context.Families.Add(family1);
        _context.Families.Add(family2);
        _context.Families.Add(family3);
        await _context.SaveChangesAsync();

        var query = new GetFamiliesQuery();

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetFamiliesQueryHandler(handlerContext, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(f => f.Id == family1.Id);
        result.Value.Should().Contain(f => f.Id == family2.Id);
        result.Value.Should().Contain(f => f.Id == family3.Id);
    }
}
