using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilyById;

public class GetFamilyByIdQueryHandlerTests : TestBase
{
    private readonly GetFamilyByIdQueryHandler _handler;
    public GetFamilyByIdQueryHandlerTests()
    {
        _handler = new GetFamilyByIdQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về đúng gia đình khi tìm thấy bằng ID.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một ID gia đình hợp lệ được cung cấp,
    /// handler sẽ truy xuất và trả về thông tin chi tiết của gia đình đó.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Family_When_Found()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình và thành viên hiện có để đảm bảo môi trường test sạch.
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Thêm một gia đình vào cơ sở dữ liệu.
        var familyId = Guid.NewGuid();
        var familyName = "Gia đình Tìm kiếm";
        _context.Families.Add(new backend.Domain.Entities.Family { Id = familyId, Name = familyName, Code = "TESTFAM" });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetFamilyByIdQuery(familyId), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo kết quả không null và các thuộc tính khớp với gia đình đã thêm.
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(familyId);
        result.Value.Name.Should().Be(familyName);
    }

    /// <summary>
    /// Kiểm tra xem có ném ra ngoại lệ NotFoundException khi không tìm thấy gia đình.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một ID gia đình không tồn tại được cung cấp,
    /// handler sẽ ném ra một NotFoundException.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Xóa tất cả các gia đình và thành viên hiện có để đảm bảo môi trường test sạch.
        _context.Members.RemoveRange(_context.Members);
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo một ID không tồn tại.
        var nonExistentFamilyId = Guid.NewGuid();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetFamilyByIdQuery(nonExistentFamilyId), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thất bại và thông báo lỗi chứa chuỗi "Family with ID {nonExistentFamilyId} not found.".
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {nonExistentFamilyId} not found.");
    }
}
