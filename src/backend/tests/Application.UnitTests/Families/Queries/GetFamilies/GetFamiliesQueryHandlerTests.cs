using AutoFixture;
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.GetFamilies;

public class GetFamiliesQueryHandlerTests : TestBase
{
    private readonly GetFamiliesQueryHandler _handler;

    public GetFamiliesQueryHandlerTests()
    {
        _handler = new GetFamiliesQueryHandler(_context, _mapper, _mockUser.Object, _mockAuthorizationService.Object);
    }



    [Fact]
    public async Task Handle_ShouldReturnAllFamilies_WhenUserIsAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về tất cả các gia đình
        // khi người dùng là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về một User.Id hợp lệ.
        // 2. Thiết lập _mockAuthorizationService.IsAdmin để trả về true.
        // 3. Thêm một số gia đình vào cơ sở dữ liệu.
        // 4. Tạo một GetFamiliesQuery bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về chứa tất cả các gia đình trong DB.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);

        var families = _fixture.CreateMany<Family>(5).ToList();
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = _fixture.Build<GetFamiliesQuery>()
                            .With(q => q.Page, 1)
                            .Without(q => q.SearchTerm)
                            .With(q => q.SortBy, "Name")
                            .With(q => q.SortOrder, "asc")
                            .With(q => q.ItemsPerPage, 100)
                            .Create();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(families.Count);
        result.Value!.Select(f => f.Id).Should().BeEquivalentTo(families.Select(f => f.Id));

        // 💡 Giải thích:
        // Test này đảm bảo rằng quản trị viên có thể xem tất cả các gia đình trong hệ thống,
        // bỏ qua các kiểm tra quyền truy cập cụ thể của người dùng.
    }

    [Fact]
    public async Task Handle_ShouldReturnFamiliesManagedByUser_WhenUserIsNotAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về chỉ các gia đình mà người dùng có quyền quản lý
        // khi người dùng không phải là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và một số Family.
        // 2. Tạo FamilyUser để liên kết UserProfile với một số Family cụ thể.
        // 3. Thiết lập _mockUser để trả về User.Id của người dùng.
        // 4. Thiết lập _mockAuthorizationService.IsAdmin để trả về false.
        // 5. Tạo một GetFamiliesQuery bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về chỉ chứa các gia đình mà người dùng có quyền quản lý.

        // Arrange
        _context.Families.RemoveRange(_context.Families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), ExternalId = userId.ToString(), Email = "test@example.com", Name = "Test User" };
        _context.UserProfiles.Add(userProfile);

        var managedFamily1 = new Family { Id = Guid.NewGuid(), Name = "Managed Family 1", Code = "MF1" };
        var managedFamily2 = new Family { Id = Guid.NewGuid(), Name = "Managed Family 2", Code = "MF2" };
        var unmanagedFamily = new Family { Id = Guid.NewGuid(), Name = "Unmanaged Family", Code = "UF1" };
        _context.Families.AddRange(managedFamily1, managedFamily2, unmanagedFamily);

        _context.FamilyUsers.Add(new FamilyUser { FamilyId = managedFamily1.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Manager });
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = managedFamily2.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Viewer });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);


        var query = _fixture
        .Build<GetFamiliesQuery>()
        .With(q => q.Page, 1)
        .Without(q => q.SearchTerm)
        .With(q => q.SortBy, "Name")
        .With(q => q.SortOrder, "asc")
        .With(q => q.ItemsPerPage, 100)
        .Create();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // managedFamily1 and managedFamily2
        result.Value.Should().Contain(f => f.Id == managedFamily1.Id);
        result.Value.Should().Contain(f => f.Id == managedFamily2.Id);
        result.Value.Should().NotContain(f => f.Id == unmanagedFamily.Id);

        // 💡 Giải thích:
        // Test này đảm bảo rằng người dùng không phải là quản trị viên chỉ có thể xem
        // các gia đình mà họ có liên kết thông qua FamilyUser, tuân thủ các quy tắc quyền truy cập.
    }

    [Fact]
    public async Task Handle_ShouldApplySearchTerm()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler lọc các gia đình dựa trên thuật ngữ tìm kiếm được cung cấp.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về một User.Id hợp lệ.
        // 2. Thiết lập _mockAuthorizationService.IsAdmin để trả về true.
        // 3. Thêm một số gia đình vào cơ sở dữ liệu, một số khớp với thuật ngữ tìm kiếm.
        // 4. Tạo một GetFamiliesQuery với thuật ngữ tìm kiếm.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về chỉ chứa các gia đình khớp với thuật ngữ tìm kiếm.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);

        var family1 = _fixture.Build<Family>().With(f => f.Name, "Family Alpha").Create();
        var family2 = _fixture.Build<Family>().With(f => f.Name, "Family Beta").Create();
        var family3 = _fixture.Build<Family>().With(f => f.Name, "Another Family").Create();
        _context.Families.AddRange(family1, family2, family3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = _fixture.Build<GetFamiliesQuery>()
                            .With(q => q.SearchTerm, "Alpha")
                            .With(q => q.Page, 1)
                            .Without(q => q.SortBy)
                            .Without(q => q.SortOrder)
                            .With(q => q.ItemsPerPage, 100)
                            .Create();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Id.Should().Be(family1.Id);

        // 💡 Giải thích:
        // Test này đảm bảo rằng chức năng tìm kiếm hoạt động chính xác,
        // chỉ trả về các gia đình có tên khớp với thuật ngữ tìm kiếm.
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler áp dụng phân trang chính xác.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về một User.Id hợp lệ.
        // 2. Thiết lập _mockAuthorizationService.IsAdmin để trả về true.
        // 3. Thêm nhiều gia đình vào cơ sở dữ liệu.
        // 4. Tạo một GetFamiliesQuery với các tham số phân trang.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem danh sách gia đình trả về có số lượng mục chính xác và các mục đúng.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);

        var families = _fixture.CreateMany<Family>(10).OrderBy(f => f.Name).ToList();
        _context.Families.AddRange(families);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = _fixture.Build<GetFamiliesQuery>()
                            .With(q => q.Page, 2)
                            .With(q => q.ItemsPerPage, 3)
                            .With(q => q.SortBy, "Name")
                            .With(q => q.SortOrder, "asc")
                            .Without(q => q.SearchTerm)
                            .Create();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value!.First().Id.Should().Be(families[3].Id);
        result.Value!.Last().Id.Should().Be(families[5].Id);

        // 💡 Giải thích:
        // Test này đảm bảo rằng chức năng phân trang hoạt động chính xác,
        // trả về đúng số lượng mục và các mục chính xác cho trang được yêu cầu.
    }
}
