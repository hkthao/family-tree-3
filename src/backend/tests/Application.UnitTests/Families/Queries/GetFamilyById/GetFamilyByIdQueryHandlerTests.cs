using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GetFamilyByIdQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về thông tin gia đình chính xác khi người dùng có quyền truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFamilyDetailDto_WhenUserHasAccess()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001",
            Description = "A test family",
            Address = "123 Test St",
            Visibility = "Private",
            TotalMembers = 5,
            TotalGenerations = 2
        };

        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync();

        var query = new GetFamilyByIdQuery(familyId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetFamilyByIdQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(familyId);
        result.Value.Name.Should().Be(existingFamily.Name);
        result.Value.Code.Should().Be(existingFamily.Code);
        result.Value.Description.Should().Be(existingFamily.Description);
        result.Value.Address.Should().Be(existingFamily.Address);
        result.Value.Visibility.Should().Be(existingFamily.Visibility);
        result.Value.TotalMembers.Should().Be(existingFamily.TotalMembers);
        result.Value.TotalGenerations.Should().Be(existingFamily.TotalGenerations);
    }
}
