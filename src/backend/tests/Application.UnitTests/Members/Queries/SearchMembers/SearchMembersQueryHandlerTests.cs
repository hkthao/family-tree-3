using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.SearchMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Queries.SearchMembers;

public class SearchMembersQueryHandlerTests : TestBase
{
    public SearchMembersQueryHandlerTests()
    {
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách phân trang khi không áp dụng bộ lọc.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenNoFiltersApplied()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        for (int i = 0; i < 10; i++)
        {
            _context.Members.Add(new Member($"LastName{i}", $"FirstName{i}", $"CODE{i}", familyId));
        }
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { Page = 1, ItemsPerPage = 5 };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(5);
        result.Value!.TotalItems.Should().Be(10);
        result.Value!.Page.Should().Be(1);
        result.Value!.TotalPages.Should().Be(2);
    }

    /// <summary>
    /// Kiểm tra xem handler có lọc thành viên theo thuật ngữ tìm kiếm.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        _context.Members.Add(new Member("Doe", "John", "JD001", familyId));
        _context.Members.Add(new Member("Smith", "Jane", "JS001", familyId));
        _context.Members.Add(new Member("Brown", "Peter", "PB001", familyId));
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { SearchQuery = "john", Page = 1, ItemsPerPage = 10 };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.First().FullName.Should().Be("John Doe");
    }

    /// <summary>
    /// Kiểm tra xem handler có lọc thành viên theo giới tính.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByGender()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        _context.Members.Add(new Member("Doe", "John", "JD001", familyId) { Gender = "Male" });
        _context.Members.Add(new Member("Smith", "Jane", "JS001", familyId) { Gender = "Female" });
        _context.Members.Add(new Member("Brown", "Peter", "PB001", familyId) { Gender = "Male" });
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { Gender = "Female", Page = 1, ItemsPerPage = 10 };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.First().FullName.Should().Be("Jane Smith");
    }

    /// <summary>
    /// Kiểm tra xem handler có lọc thành viên theo FamilyId.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByFamilyId()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Test Family 1", Code = "TF001" };
        var family2 = new Family { Id = family2Id, Name = "Test Family 2", Code = "TF002" };
        _context.Families.AddRange(family1, family2);

        _context.Members.Add(new Member("Doe", "John", "JD001", family1Id));
        _context.Members.Add(new Member("Smith", "Jane", "JS001", family1Id));
        _context.Members.Add(new Member("Brown", "Peter", "PB001", family2Id));
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { FamilyId = family1Id, Page = 1, ItemsPerPage = 10 };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items.Should().Contain(m => m.FullName == "John Doe");
        result.Value!.Items.Should().Contain(m => m.FullName == "Jane Smith");
        result.Value!.Items.Should().NotContain(m => m.FullName == "Peter Brown");
    }

    /// <summary>
    /// Kiểm tra xem handler có áp dụng sắp xếp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldApplySorting()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        _context.Members.Add(new Member("Doe", "John", "JD001", familyId));
        _context.Members.Add(new Member("Smith", "Jane", "JS001", familyId));
        _context.Members.Add(new Member("Brown", "Peter", "PB001", familyId));
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { SortBy = "FirstName", SortOrder = "desc", Page = 1, ItemsPerPage = 10 };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value!.Items.First().FullName.Should().Be("Peter Brown");
        result.Value!.Items.Skip(1).First().FullName.Should().Be("John Doe");
        result.Value!.Items.Skip(2).First().FullName.Should().Be("Jane Smith");
    }

    /// <summary>
    /// Kiểm tra xem handler có áp dụng tất cả các bộ lọc và sắp xếp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldApplyAllFiltersAndSorting()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Test Family 1", Code = "TF001" };
        var family2 = new Family { Id = family2Id, Name = "Test Family 2", Code = "TF002" };
        _context.Families.AddRange(family1, family2);

        _context.Members.Add(new Member("Doe", "John", "JD001", family1Id) { Gender = "Male" });
        _context.Members.Add(new Member("Smith", "Jane", "JS001", family1Id) { Gender = "Female" });
        _context.Members.Add(new Member("Brown", "Peter", "PB001", family2Id) { Gender = "Male" });
        _context.Members.Add(new Member("White", "Alice", "AW001", family1Id) { Gender = "Female" });
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery
        {
            SearchQuery = "a", // Matches Jane and Alice
            Gender = "Female",
            FamilyId = family1Id,
            SortBy = "FirstName",
            SortOrder = "asc",
            Page = 1,
            ItemsPerPage = 10
        };

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new SearchMembersQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items.First().FullName.Should().Be("Alice White"); // Alice comes before Jane by FirstName asc
        result.Value!.Items.Skip(1).First().FullName.Should().Be("Jane Smith");
    }
}
