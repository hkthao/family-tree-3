using AutoFixture;
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{
    private readonly SearchFamiliesQueryHandler _handler;

    public SearchFamiliesQueryHandlerTests()
    {
        _handler = new SearchFamiliesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenNoSearchCriteriaProvided()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một danh sách phân trang các gia đình
        // khi không có tiêu chí tìm kiếm nào được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thêm một số gia đình vào cơ sở dữ liệu.
        // 2. Tạo một SearchFamiliesQuery mặc định (không có tiêu chí tìm kiếm).
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách phân trang có chứa tất cả các gia đình trong DB.

        // Arrange
        var families = _fixture.CreateMany<Family>(5).ToList();
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(5);
        result.Value.TotalItems.Should().Be(5);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi không có bộ lọc nào được áp dụng,
        // handler sẽ trả về tất cả các gia đình hiện có trong cơ sở dữ liệu dưới dạng danh sách phân trang.
    }

    [Fact]
    public async Task Handle_ShouldApplySearchQuery()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler lọc các gia đình dựa trên thuật ngữ tìm kiếm được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thêm một số gia đình vào cơ sở dữ liệu, một số khớp với thuật ngữ tìm kiếm.
        // 2. Tạo một SearchFamiliesQuery với thuật ngữ tìm kiếm.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách phân trang chỉ chứa các gia đình khớp với thuật ngữ tìm kiếm.

        // Arrange
        var family1 = _fixture.Build<Family>().With(f => f.Name, "Family Alpha").Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, "Family Beta").Create();
        var family3 = _fixture.Build<Family>().With(f => f.Name, "Another Family").Create();
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { SearchQuery = "Alpha", Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(1);

        // 💡 Giải thích:
        // Test này đảm bảo rằng chức năng tìm kiếm hoạt động chính xác,
        // chỉ trả về các gia đình có tên khớp với thuật ngữ tìm kiếm.
    }

    [Fact]
    public async Task Handle_ShouldApplyVisibilityFilter()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler lọc các gia đình dựa trên bộ lọc hiển thị được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thêm một số gia đình vào cơ sở dữ liệu với các cài đặt hiển thị khác nhau.
        // 2. Tạo một SearchFamiliesQuery với bộ lọc hiển thị.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách phân trang chỉ chứa các gia đình khớp với bộ lọc hiển thị.

        // Arrange
        var publicFamily = _fixture.Build<Family>().With(f => f.Visibility, FamilyVisibility.Public.ToString()).Create();
        var privateFamily = _fixture.Build<Family>().With(f => f.Visibility, FamilyVisibility.Private.ToString()).Create();
        var unlistedFamily = _fixture.Build<Family>().With(f => f.Visibility, "Unlisted").Create();
        _context.Families.AddRange(publicFamily, privateFamily, unlistedFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { Visibility = FamilyVisibility.Public.ToString(), Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Id.Should().Be(publicFamily.Id);
        result.Value.TotalItems.Should().Be(1);

        // 💡 Giải thích:
        // Test này đảm bảo rằng chức năng lọc theo khả năng hiển thị hoạt động chính xác,
        // chỉ trả về các gia đình có cài đặt hiển thị khớp với bộ lọc.
    }

    [Fact]
    public async Task Handle_ShouldApplySorting()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler áp dụng sắp xếp chính xác.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thêm một số gia đình vào cơ sở dữ liệu với các tên khác nhau.
        // 2. Tạo một SearchFamiliesQuery với tham số sắp xếp.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách phân trang được sắp xếp theo thứ tự mong đợi.

        // Arrange
        var familyA = _fixture.Build<Family>().With(f => f.Name, "Family A").Create();
        var familyC = _fixture.Build<Family>().With(f => f.Name, "Family C").Create();
        var familyB = _fixture.Build<Family>().With(f => f.Name, "Family B").Create();
        _context.Families.AddRange(familyA, familyC, familyB);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { SortBy = "Name", SortOrder = "asc", Page = 1, ItemsPerPage = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items[0].Id.Should().Be(familyA.Id);
        result.Value.Items[1].Id.Should().Be(familyB.Id);
        result.Value.Items[2].Id.Should().Be(familyC.Id);

        // 💡 Giải thích:
        // Test này đảm bảo rằng chức năng sắp xếp hoạt động chính xác,
        // trả về các gia đình theo thứ tự tăng dần của tên.
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler áp dụng phân trang chính xác.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thêm nhiều gia đình vào cơ sở dữ liệu.
        // 2. Tạo một SearchFamiliesQuery với các tham số phân trang.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách phân trang có số lượng mục chính xác và các mục đúng.

        // Arrange
        var families = _fixture.CreateMany<Family>(10).OrderBy(f => f.Name).ToList();
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { Page = 2, ItemsPerPage = 3, SortBy = "Name", SortOrder = "asc" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items.First().Id.Should().Be(families[3].Id);
        result.Value.Items.Last().Id.Should().Be(families[5].Id);
        result.Value.TotalItems.Should().Be(10);

                // 💡 Giải thích:

                // Test này đảm bảo rằng chức năng phân trang hoạt động chính xác,

                // trả về đúng số lượng mục và các mục chính xác cho trang được yêu cầu.

            }

        }
