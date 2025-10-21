using AutoFixture;
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

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFamiliesFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một danh sách rỗng
        // khi không tìm thấy gia đình nào cho các ID được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một danh sách các ID gia đình không tồn tại.
        // 2. Đảm bảo không có gia đình nào trong DB khớp với các ID này.
        // 3. Tạo một GetFamiliesByIdsQuery với danh sách ID.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về là rỗng.

        // Arrange
        var nonExistentIds = _fixture.CreateMany<Guid>(3).ToList();
        var query = new GetFamiliesByIdsQuery(nonExistentIds);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi không có gia đình nào khớp với các ID được yêu cầu,
        // handler sẽ trả về một danh sách rỗng thay vì lỗi.
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFamilies_WhenFamiliesFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các gia đình chính xác
        // khi các gia đình khớp với các ID được cung cấp được tìm thấy.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một số gia đình và thêm vào DB.
        // 2. Chọn một tập hợp con các ID gia đình để truy vấn.
        // 3. Tạo một GetFamiliesByIdsQuery với các ID đã chọn.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về có số lượng và nội dung chính xác.

        // Arrange
        var allFamilies = _fixture.CreateMany<Family>(5).ToList();
        _context.Families.AddRange(allFamilies);
        await _context.SaveChangesAsync(CancellationToken.None);

        var idsToQuery = allFamilies.Take(2).Select(f => f.Id).ToList();
        var query = new GetFamiliesByIdsQuery(idsToQuery);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Select(f => f.Id).Should().BeEquivalentTo(idsToQuery);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi các gia đình khớp với các ID được yêu cầu được tìm thấy,
        // handler sẽ truy xuất và ánh xạ chúng thành công sang FamilyDto.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenEmptyIdsListProvided()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một danh sách rỗng
        // khi một danh sách ID trống được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một GetFamiliesByIdsQuery với một danh sách ID trống.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về là rỗng.

        // Arrange
        var query = new GetFamiliesByIdsQuery(new List<Guid>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        // 💡 Giải thích:
        // Test này đảm bảo rằng việc cung cấp một danh sách ID trống sẽ không gây ra lỗi
        // và sẽ trả về một danh sách gia đình trống một cách hợp lý.
    }
}
