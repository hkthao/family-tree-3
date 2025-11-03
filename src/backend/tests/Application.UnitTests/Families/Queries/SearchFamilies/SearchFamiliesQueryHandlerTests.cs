using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public SearchFamiliesQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách các gia đình phù hợp với từ khóa tìm kiếm.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenSearchTermProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var family1 = new Family { Id = Guid.NewGuid(), Name = "Nguyen Family", Code = "NGUYEN001", TotalMembers = 10, TotalGenerations = 3 };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Tran Family", Code = "TRAN001", TotalMembers = 5, TotalGenerations = 2 };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Le Family", Code = "LE001", TotalMembers = 7, TotalGenerations = 4 };

        _context.Families.Add(family1);
        _context.Families.Add(family2);
        _context.Families.Add(family3);
        await _context.SaveChangesAsync();

        var query = new SearchFamiliesQuery { SearchQuery = "Nguyen" };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchFamiliesQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.First().Id.Should().Be(family1.Id);
    }
}
