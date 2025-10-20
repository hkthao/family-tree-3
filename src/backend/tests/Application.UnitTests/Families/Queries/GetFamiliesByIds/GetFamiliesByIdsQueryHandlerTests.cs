using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamiliesByIds;

public class GetFamiliesByIdsQueryHandlerTests : TestBase
{
    private readonly GetFamiliesByIdsQueryHandler _handler;

    public GetFamiliesByIdsQueryHandlerTests()
    {
        _handler = new GetFamiliesByIdsQueryHandler(_context, _mapper);
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
    /// Kiểm tra xem có thể truy xuất các gia đình thành công bằng danh sách ID.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách các ID gia đình hợp lệ,
    /// handler sẽ trả về chính xác các gia đình tương ứng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFamilies_When_IdsExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        // Thêm một số gia đình vào cơ sở dữ liệu.
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Gia đình 1", Code = "GFBI1" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Gia đình 2", Code = "GFBI2" };
        _context.Families.Add(family1);
        _context.Families.Add(family2);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với các ID của gia đình.
        var query = new GetFamiliesByIdsQuery(new List<Guid> { family1.Id, family2.Id });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về 2 gia đình.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.Should().Contain(f => f.Id == family1.Id && f.Name == family1.Name);
        result.Value!.Should().Contain(f => f.Id == family2.Id && f.Name == family2.Name);
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các gia đình khi một số ID không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID bao gồm cả ID tồn tại và không tồn tại,
    /// handler sẽ chỉ trả về các gia đình có ID tồn tại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnOnlyExistingFamilies_When_SomeIdsDoNotExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        // Thêm một gia đình vào cơ sở dữ liệu.
        var existingFamily = new Family { Id = Guid.NewGuid(), Name = "Gia đình Tồn Tại", Code = "GFBIE" };
        _context.Families.Add(existingFamily);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với một ID tồn tại và một ID không tồn tại.
        var nonExistentId = Guid.NewGuid();
        var query = new GetFamiliesByIdsQuery(new List<Guid> { existingFamily.Id, nonExistentId });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và chỉ trả về 1 gia đình (gia đình tồn tại).
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.Id.Should().Be(existingFamily.Id);
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi không có ID nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID rỗng,
    /// handler sẽ trả về một danh sách gia đình rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoIdsProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        await ClearDatabaseAndSetupData();

        // Tạo truy vấn với danh sách ID rỗng.
        var query = new GetFamiliesByIdsQuery(new List<Guid>());

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về danh sách rỗng.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
