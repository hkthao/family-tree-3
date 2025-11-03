using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : TestBase
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly GetFamiliesQueryHandler _handler;

    public GetFamiliesQueryHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new GetFamiliesQueryHandler(_context, _mapperMock.Object, _currentUserMock.Object, _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFamilies_ForAdminUser()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB" };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetFamiliesQuery();

        // Setup mapper to return FamilyListDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyListDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.Name == "Family A");
        result.Value.Should().Contain(f => f.Name == "Family B");
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessibleFamilies_ForNonAdminUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB" };
        family1.AddFamilyUser(userId, FamilyRole.Manager);
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var query = new GetFamiliesQuery();

        // Setup mapper to return FamilyListDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyListDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(f => f.Name == "Family A");
        result.Value.Should().NotContain(f => f.Name == "Family B");
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredFamilies_WhenSearchTermIsProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family Alpha", Description = "Description for Alpha", Code = "FA" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family Beta", Description = "Description for Beta", Code = "FB" };
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetFamiliesQuery { SearchTerm = "Alpha" };

        // Setup mapper to return FamilyListDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyListDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Name.Should().Be("Family Alpha");
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderedFamilies_WhenSortByAndSortOrderAreProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family C", Code = "FC" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB" };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetFamiliesQuery { SortBy = "Name", SortOrder = "asc" };

        // Setup mapper to return FamilyListDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyListDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value!.First().Name.Should().Be("Family A");
        result.Value!.Last().Name.Should().Be("Family C");
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedFamilies_WhenPageAndItemsPerPageAreProvided()
    {
        // Arrange
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family 1", Code = "F1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family 2", Code = "F2" };
        var family3 = new Family { Id = Guid.NewGuid(), Name = "Family 3", Code = "F3" };
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetFamiliesQuery { Page = 1, ItemsPerPage = 2 };

        // Setup mapper to return FamilyListDto
        _mapperMock.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Family, FamilyListDto>();
        }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.First().Name.Should().Be("Family 1");
        result.Value!.Last().Name.Should().Be("Family 2");
    }
}
