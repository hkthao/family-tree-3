using backend.Application.Members.Queries.SearchMembers;
using backend.Application.UnitTests.Common;
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



    /// <summary>
    /// Kiểm tra tìm kiếm thành viên theo truy vấn tìm kiếm (tên hoặc biệt danh).
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các thành viên dựa trên một chuỗi tìm kiếm
    /// khớp với tên hoặc biệt danh của thành viên.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Members_BySearchQuery()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { SearchQuery = "Prince", Page = 1, ItemsPerPage = 100, FamilyId = royalFamilyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(12);
        result.Value.Items.Should().Contain(x => x.FullName.Contains("William"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("George"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Louis"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Harry"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Philip"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Andrew"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Catherine"));
    }

    /// <summary>
    /// Kiểm tra tìm kiếm thành viên theo FamilyId.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các thành viên dựa trên ID của gia đình.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Members_ByFamilyId()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { FamilyId = royalFamilyId, Page = 1, ItemsPerPage = 1000 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(19);
    }

    /// <summary>
    /// Kiểm tra tìm kiếm thành viên theo giới tính.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể lọc các thành viên dựa trên giới tính.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Members_ByGender()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { Gender = "Male", Page = 1, ItemsPerPage = 10, FamilyId = royalFamilyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(8);
        result.Value.Items.Should().Contain(x => x.FullName.Contains("William"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("George"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Louis"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Harry"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Philip"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Andrew"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Archie"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Charles III"));
    }

    /// <summary>
    /// Kiểm tra tìm kiếm thành viên với nhiều tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng handler có thể kết hợp nhiều tiêu chí tìm kiếm (ví dụ: truy vấn và FamilyId)
    /// để lọc các thành viên.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Matching_Members_ByMultipleCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { SearchQuery = "Prince", FamilyId = royalFamilyId, Gender = "Male", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(6);
        result.Value.Items.Should().Contain(x => x.FullName.Contains("William"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("George"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Louis"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Harry"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Philip"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Andrew"));
    }

    /// <summary>
    /// Kiểm tra trả về danh sách rỗng khi không có thành viên nào khớp với tiêu chí.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi không có thành viên nào trong cơ sở dữ liệu khớp với các tiêu chí tìm kiếm,
    /// handler sẽ trả về một danh sách thành viên rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoMembersMatchCriteria()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { SearchQuery = "NonExistent", Page = 1, ItemsPerPage = 10, FamilyId = royalFamilyId };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trả về tất cả các thành viên khi không có tiêu chí tìm kiếm nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một truy vấn tìm kiếm rỗng được gửi đi,
    /// handler sẽ trả về tất cả các thành viên có trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnAllMembers_When_NoSearchCriteriaProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        var query = new SearchMembersQuery { Page = 1, ItemsPerPage = 1000, FamilyId = royalFamilyId }; // Không có tiêu chí tìm kiếm

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(19);
    }
}
