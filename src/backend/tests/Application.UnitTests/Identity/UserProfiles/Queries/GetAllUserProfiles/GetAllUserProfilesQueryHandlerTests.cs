using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Identity.UserProfiles.Queries.GetAllUserProfiles;

public class GetAllUserProfilesQueryHandlerTests : TestBase
{
    private readonly GetAllUserProfilesQueryHandler _handler;

    public GetAllUserProfilesQueryHandlerTests()
    {


        _handler = new GetAllUserProfilesQueryHandler(
            _context,
            _mapper
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUserProfiles()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về tất cả hồ sơ người dùng hiện có.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm một số UserProfile vào Context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa tất cả các hồ sơ người dùng đã thêm.
        var userProfiles = _fixture.CreateMany<UserProfile>(3).ToList();
        _context.UserProfiles.AddRange(userProfiles);
        await _context.SaveChangesAsync();

        var query = new GetAllUserProfilesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value!.Select(up => up.ExternalId).Should().Contain(userProfiles.Select(up => up.ExternalId));
        // 💡 Giải thích: Handler phải truy xuất và trả về tất cả hồ sơ người dùng từ cơ sở dữ liệu.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoUserProfiles()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi không có hồ sơ người dùng nào.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Đảm bảo không có UserProfile nào trong Context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa một danh sách rỗng.
        var query = new GetAllUserProfilesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Khi không có hồ sơ người dùng nào trong cơ sở dữ liệu, handler phải trả về một danh sách rỗng.
    }
}
