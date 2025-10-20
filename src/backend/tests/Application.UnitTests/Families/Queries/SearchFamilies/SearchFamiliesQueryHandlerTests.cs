using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.SearchFamilies;

public class SearchFamiliesQueryHandlerTests : TestBase
{
    private readonly SearchFamiliesQueryHandler _handler;

    public SearchFamiliesQueryHandlerTests()
    {
        _handler = new SearchFamiliesQueryHandler(_context, _mapper);
    }

    private async Task ClearDatabaseAndSetupData()
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Kiểm tra tìm kiếm gia đình theo truy vấn tìm kiếm (tên, mô tả hoặc địa chỉ).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các gia đình dựa trên một chuỗi tìm kiếm
    /// khớp với tên, mô tả hoặc địa chỉ của gia đình.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Families_BySearchQuery()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        _context.Families.AddRange(
            new Family { Id = Guid.NewGuid(), Name = "Gia đình A", Description = "Mô tả A", Address = "Địa chỉ A", Code = "FSA" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình B", Description = "Mô tả B", Address = "Địa chỉ B", Code = "FSB" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình C", Description = "Mô tả C", Address = "Địa chỉ C", Code = "FSC" }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { SearchQuery = "Gia đình A", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.Name.Should().Be("Gia đình A");
    }

    /// <summary>
    /// Kiểm tra tìm kiếm gia đình theo Visibility.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các gia đình dựa trên cài đặt hiển thị (Visibility).
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Families_ByVisibility()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        _context.Families.AddRange(
            new Family { Id = Guid.NewGuid(), Name = "Gia đình Công khai", Visibility = "Public", Code = "FSPUB" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình Riêng tư", Visibility = "Private", Code = "FSPRIV" }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { Visibility = "Public", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.Name.Should().Be("Gia đình Công khai");
    }

    /// <summary>
    /// Kiểm tra tìm kiếm gia đình với nhiều tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể kết hợp nhiều tiêu chí tìm kiếm (ví dụ: truy vấn và Visibility)
    /// để lọc các gia đình.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Families_ByMultipleCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        _context.Families.AddRange(
            new Family { Id = Guid.NewGuid(), Name = "Gia đình A Công khai", Visibility = "Public", Code = "FSAMC" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình B Riêng tư", Visibility = "Private", Code = "FSBMC" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình C Công khai", Visibility = "Public", Code = "FSCCC" }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { SearchQuery = "Gia đình A", Visibility = "Public", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.Name.Should().Be("Gia đình A Công khai");
    }

    /// <summary>
    /// Kiểm tra trả về danh sách rỗng khi không có gia đình nào khớp với tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi không có gia đình nào trong cơ sở dữ liệu khớp với các tiêu chí tìm kiếm,
    /// handler sẽ trả về một danh sách gia đình rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoFamiliesMatchCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        _context.Families.Add(new Family { Id = Guid.NewGuid(), Name = "Gia đình Duy Nhất", Visibility = "Private", Code = "FSDN" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { SearchQuery = "Gia đình Không Tồn Tại", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trả về tất cả các gia đình khi không có tiêu chí tìm kiếm nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một truy vấn tìm kiếm rỗng được gửi đi,
    /// handler sẽ trả về tất cả các gia đình có trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnAllFamilies_When_NoSearchCriteriaProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        _context.Families.AddRange(
            new Family { Id = Guid.NewGuid(), Name = "Gia đình 1", Visibility = "Public", Code = "FS1" },
            new Family { Id = Guid.NewGuid(), Name = "Gia đình 2", Visibility = "Private", Code = "FS2" }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchFamiliesQuery { Page = 1, ItemsPerPage = 10 }; // Không có tiêu chí tìm kiếm

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }
}
