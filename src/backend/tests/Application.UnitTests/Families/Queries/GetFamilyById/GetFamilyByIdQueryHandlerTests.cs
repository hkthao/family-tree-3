using AutoFixture.Xunit2;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries;

public class GetFamilyByIdQueryHandlerTests : TestBase
{
    private readonly GetFamilyByIdQueryHandler _handler;

    public GetFamilyByIdQueryHandlerTests()
    {
        _handler = new GetFamilyByIdQueryHandler(_context, _mapper);
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnFailureResult_WhenFamilyNotFound(GetFamilyByIdQuery query)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về lỗi khi không tìm thấy Family với ID được yêu cầu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Đảm bảo không có Family nào trong Context với ID của query.
        // (Mặc định Context sẽ trống rỗng, không cần thêm Family nào có ID trùng với query.Id)

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {query.Id} not found.");

        // 💡 Giải thích: Khi không tìm thấy Family trong cơ sở dữ liệu với ID đã cho,
        // handler sẽ trả về Result.Failure với thông báo lỗi tương ứng.
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnFamilyDetailDto_WhenFamilyFound(GetFamilyByIdQuery query)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về FamilyDetailDto chính xác khi tìm thấy Family.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Tạo một Family và thêm vào Context.
        var family = new Family
        {
            Id = query.Id,
            Name = "Test Family",
            Code = "TF123",
            Description = "A test family description",
            Address = "123 Test St",
            AvatarUrl = "http://example.com/avatar.jpg",
            Visibility = "Public",
            TotalMembers = 5
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa FamilyDetailDto đã ánh xạ chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(family.Id);
        result.Value!.Name.Should().Be(family.Name);
        result.Value!.Description.Should().Be(family.Description);
        result.Value!.Address.Should().Be(family.Address);
        result.Value!.AvatarUrl.Should().Be(family.AvatarUrl);
        result.Value!.Visibility.Should().Be(family.Visibility);
        result.Value!.TotalMembers.Should().Be(family.TotalMembers);

        // 💡 Giải thích: Khi tìm thấy Family trong cơ sở dữ liệu với ID đã cho,
        // handler sẽ ánh xạ nó sang FamilyDetailDto và trả về Result.Success.
    }
}
