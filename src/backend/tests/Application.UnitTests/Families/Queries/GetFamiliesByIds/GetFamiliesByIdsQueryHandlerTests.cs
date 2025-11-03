using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace backend.Application.UnitTests.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GetFamiliesByIdsQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách các gia đình chính xác khi cung cấp một danh sách các ID hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFamilies_WhenValidIdsProvided()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var familyId3 = Guid.NewGuid();

        var family1 = new Family { Id = familyId1, Name = "Family 1", Code = "FAM001", TotalMembers = 10, TotalGenerations = 3 };
        var family2 = new Family { Id = familyId2, Name = "Family 2", Code = "FAM002", TotalMembers = 5, TotalGenerations = 2 };
        var family3 = new Family { Id = familyId3, Name = "Family 3", Code = "FAM003", TotalMembers = 7, TotalGenerations = 4 };

        _context.Families.Add(family1);
        _context.Families.Add(family2);
        _context.Families.Add(family3);
        await _context.SaveChangesAsync();

        var query = new GetFamiliesByIdsQuery(new List<Guid> { familyId1, familyId2 });

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetFamiliesByIdsQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.Id == familyId1);
        result.Value.Should().Contain(f => f.Id == familyId2);
        result.Value.Should().NotContain(f => f.Id == familyId3);
    }
}
