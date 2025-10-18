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

    private async Task ClearDatabaseAndSetupData(Guid familyId)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);
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
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        _context.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Nickname = "Johnny", FamilyId = familyId },
            new Member { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Nickname = "Janey", FamilyId = familyId },
            new Member { Id = Guid.NewGuid(), FirstName = "Peter", LastName = "Jones", Nickname = "Pete", FamilyId = familyId }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { SearchQuery = "Doe", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Doe John"));
        result.Value.Items.Should().Contain(x => x.FullName.Contains("Doe Jane"));
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
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId1);
        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2" });
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "A", FamilyId = familyId1 },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "B", FamilyId = familyId2 }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { FamilyId = familyId1, Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.FullName.Should().Be("A Member");
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
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        _context.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FirstName = "Male", LastName = "Member", Gender = "Male", FamilyId = familyId },
            new Member { Id = Guid.NewGuid(), FirstName = "Female", LastName = "Member", Gender = "Female", FamilyId = familyId }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { Gender = "Male", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.FullName.Should().Be("Member Male");
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
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId1);
        _context.Families.Add(new Family { Id = familyId2, Name = "Family 2" });
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", FamilyId = familyId1, Gender = "Male" },
            new Member { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", FamilyId = familyId1, Gender = "Female" },
            new Member { Id = Guid.NewGuid(), FirstName = "John", LastName = "Smith", FamilyId = familyId2, Gender = "Male" }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { SearchQuery = "John", FamilyId = familyId1, Gender = "Male", Page = 1, ItemsPerPage = 10 };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First()!.FullName.Should().Be("Doe John");
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
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        _context.Members.Add(new Member { Id = Guid.NewGuid(), FirstName = "Unique", LastName = "Member", FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { SearchQuery = "NonExistent", Page = 1, ItemsPerPage = 10 };

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
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        _context.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "1", FamilyId = familyId },
            new Member { Id = Guid.NewGuid(), FirstName = "Member", LastName = "2", FamilyId = familyId }
        );
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new SearchMembersQuery { Page = 1, ItemsPerPage = 10 }; // Không có tiêu chí tìm kiếm

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }
}
