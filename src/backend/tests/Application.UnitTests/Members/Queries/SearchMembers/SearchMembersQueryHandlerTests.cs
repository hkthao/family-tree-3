using backend.Application.Members.Queries.SearchMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.SearchMembers;

public class SearchMembersQueryHandlerTests : TestBase
{
    private readonly SearchMembersQueryHandler _handler;

    public SearchMembersQueryHandlerTests()
    {
        _handler = new SearchMembersQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllMembers_WhenNoFiltersProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về tất cả thành viên khi không có bộ lọc nào được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều gia đình và thành viên vào Context.
        // 2. Act: Gọi phương thức Handle với SearchMembersQuery không có bộ lọc.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa tất cả thành viên, được phân trang đúng.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001", Gender = "Male" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Jane", LastName = "Doe", Code = "M002", Gender = "Female" };
        _context.Families.Add(family1);
        _context.Members.AddRange(member1, member2);

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Peter", LastName = "Pan", Code = "M003", Gender = "Male" };
        _context.Families.Add(family2);
        _context.Members.Add(member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery(); // No filters

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value!.TotalItems.Should().Be(3);
        // 💡 Giải thích: Khi không có bộ lọc, handler sẽ trả về tất cả thành viên.
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchQuery()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc thành viên theo SearchQuery.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên với tên khác nhau vào Context.
        // 2. Act: Gọi phương thức Handle với SearchMembersQuery có SearchQuery.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên khớp với SearchQuery.
        var family = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Smith", Code = "M002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Peter", LastName = "Jones", Code = "M003" };
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { SearchQuery = "john" }; // Case-insensitive search

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.Should().Contain(m => m.Id == member1.Id);
        // 💡 Giải thích: Handler phải lọc thành viên theo SearchQuery (không phân biệt chữ hoa chữ thường).
    }

    [Fact]
    public async Task Handle_ShouldFilterByGender()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc thành viên theo giới tính.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên với giới tính khác nhau vào Context.
        // 2. Act: Gọi phương thức Handle với SearchMembersQuery có Gender filter.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên khớp với giới tính.
        var family = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001", Gender = "Male" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Doe", Code = "M002", Gender = "Female" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Peter", LastName = "Pan", Code = "M003", Gender = "Male" };
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { Gender = "Female" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.Should().Contain(m => m.Id == member2.Id);
        // 💡 Giải thích: Handler phải lọc thành viên theo giới tính.
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamilyId()
    {
        // 🎯 Mục tiêu của test: Xác minh handler lọc thành viên theo FamilyId.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm thành viên vào các gia đình khác nhau.
        // 2. Act: Gọi phương thức Handle với SearchMembersQuery có FamilyId filter.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa thành viên từ FamilyId đó.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Jane", LastName = "Doe", Code = "M002" };
        _context.Families.Add(family1);
        _context.Members.AddRange(member1, member2);

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Peter", LastName = "Pan", Code = "M003" };
        _context.Families.Add(family2);
        _context.Members.Add(member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { FamilyId = family1.Id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items.Should().Contain(m => m.Id == member1.Id);
        result.Value!.Items.Should().Contain(m => m.Id == member2.Id);
        result.Value!.Items.Should().NotContain(m => m.Id == member3.Id);
        // 💡 Giải thích: Handler phải lọc thành viên theo FamilyId.
    }

    [Fact]
    public async Task Handle_ShouldApplyOrdering()
    {
        // 🎯 Mục tiêu của test: Xác minh handler áp dụng sắp xếp đúng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên với tên khác nhau vào Context.
        // 2. Act: Gọi phương thức Handle với SortBy và SortOrder.
        // 3. Assert: Kiểm tra kết quả trả về được sắp xếp đúng.
        var family = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Smith", Code = "M002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Peter", LastName = "Jones", Code = "M003" };
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { SortBy = "FullName", SortOrder = "asc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value!.Items.First().FullName.Should().Contain("Jane"); // Jane, John, Peter
        result.Value!.Items.Last().FullName.Should().Contain("Peter");
        // 💡 Giải thích: Handler phải sắp xếp thành viên theo trường và thứ tự được chỉ định.
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // 🎯 Mục tiêu của test: Xác minh handler áp dụng phân trang đúng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên vào Context.
        // 2. Act: Gọi phương thức Handle với Page và ItemsPerPage.
        // 3. Assert: Kiểm tra kết quả trả về chứa đúng số lượng mục cho trang và tổng số lượng.
        var family = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        _context.Families.Add(family);

        for (int i = 0; i < 10; i++)
        {
            _context.Members.Add(new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = $"Member{i}", LastName = "Test", Code = $"M{i}" });
        }
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery { Page = 2, ItemsPerPage = 3, SortBy = "FirstName", SortOrder = "asc" }; // Get page 2, 3 items per page

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value!.TotalItems.Should().Be(10);
        result.Value!.Page.Should().Be(2);
        result.Value!.TotalPages.Should().Be(4); // 10 items, 3 per page = 4 pages
        // 💡 Giải thích: Handler phải trả về các mục được phân trang đúng.
    }

    [Fact]
    public async Task Handle_ShouldCombineFilters()
    {
        // 🎯 Mục tiêu của test: Xác minh handler kết hợp các bộ lọc đúng.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên với các thuộc tính khác nhau vào Context.
        // 2. Act: Gọi phương thức Handle với SearchMembersQuery có nhiều bộ lọc.
        // 3. Assert: Kiểm tra kết quả trả về chỉ chứa thành viên khớp với tất cả các bộ lọc.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", Code = "FA001" };
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "John", LastName = "Doe", Code = "M001", Gender = "Male" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Jane", LastName = "Doe", Code = "M002", Gender = "Female" };
        _context.Families.Add(family1);
        _context.Members.AddRange(member1, member2);

        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", Code = "FB001" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Peter", LastName = "Pan", Code = "M003", Gender = "Male" };
        _context.Families.Add(family2);
        _context.Members.Add(member3);
        await _context.SaveChangesAsync();

        var query = new SearchMembersQuery
        {
            SearchQuery = "john",
            Gender = "Male",
            FamilyId = family1.Id
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value!.Items.Should().Contain(m => m.Id == member1.Id);
        // 💡 Giải thích: Handler phải kết hợp các bộ lọc để trả về kết quả chính xác.
    }
}
