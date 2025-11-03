// File: GetMembersQueryHandlerTests.cs
// Mô tả:
// File này chứa các bài kiểm tra đơn vị (unit tests) cho lớp `GetMembersQueryHandler`.
// `GetMembersQueryHandler` chịu trách nhiệm xử lý các truy vấn để lấy danh sách thành viên.
// Các bài kiểm tra này đảm bảo rằng logic nghiệp vụ trong handler hoạt động chính xác
// trong các trường hợp khác nhau, bao gồm cả việc kiểm tra quyền truy cập của người dùng.

using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

/// <summary>
/// Lớp kiểm thử đơn vị cho <see cref="GetMembersQueryHandler"/>.
/// Kế thừa từ <see cref="TestBase"/> để sử dụng các thiết lập chung cho kiểm thử.
/// </summary>
public class GetMembersQueryHandlerTests : TestBase
{
    // Mock đối tượng ICurrentUser để giả lập thông tin người dùng hiện tại
    private readonly Mock<ICurrentUser> _currentUserMock;
    // Mock đối tượng IAuthorizationService để giả lập dịch vụ phân quyền
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp <see cref="GetMembersQueryHandlerTests"/>.
    /// Thiết lập các mock đối tượng cần thiết cho mỗi bài kiểm thử.
    /// </summary>
    public GetMembersQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về tất cả thành viên khi người dùng là quản trị viên (Admin) hay không.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllMembers_ForAdminUser()
    {
        // Arrange (Thiết lập môi trường kiểm thử)
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();

        // Thêm các gia đình vào cơ sở dữ liệu giả lập
        _context.Families.AddRange(
            new Family { Id = familyId1, Name = "Doe", Code = "DOE" },
            new Family { Id = familyId2, Name = "Smith", Code = "SMI" }
        );

        // Thêm các thành viên vào cơ sở dữ liệu giả lập
        _context.Members.AddRange(
            new Member("John", "Doe", "JD1", familyId1),
            new Member("Jane", "Doe", "JD2", familyId2)
        );
        await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

        // Giả lập rằng người dùng hiện tại là quản trị viên
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        // Khởi tạo handler với các dependency đã mock
        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);
        var query = new GetMembersQuery(); // Tạo một truy vấn mới

        // Act (Thực thi hành động cần kiểm thử)
        var result = await handler.Handle(query, CancellationToken.None); // Gọi phương thức Handle của handler

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue(); // Đảm bảo truy vấn thành công
        result.Value.Should().HaveCount(2); // Đảm bảo trả về đúng 2 thành viên
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các thành viên từ các gia đình mà người dùng không phải quản trị viên có quyền truy cập hay không.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMembersFromAccessibleFamilies_ForNonAdminUser()
    {
        // Arrange (Thiết lập môi trường kiểm thử)
        var userId = Guid.NewGuid(); // ID của người dùng hiện tại
        var accessibleFamilyId = Guid.NewGuid(); // ID của gia đình mà người dùng có quyền truy cập
        var inaccessibleFamilyId = Guid.NewGuid(); // ID của gia đình mà người dùng không có quyền truy cập

        // Thêm các gia đình vào cơ sở dữ liệu giả lập
        _context.Families.AddRange(
            new Family { Id = accessibleFamilyId, Name = "Accessible Family", Code = "ACC" },
            new Family { Id = inaccessibleFamilyId, Name = "Inaccessible Family", Code = "INACC" }
        );

        // Thêm mối quan hệ người dùng-gia đình vào cơ sở dữ liệu giả lập
        _context.FamilyUsers.Add(new FamilyUser(accessibleFamilyId, userId, FamilyRole.Viewer));
        // Thêm các thành viên vào cơ sở dữ liệu giả lập
        _context.Members.AddRange(
            new Member("John", "Doe", "JD1", accessibleFamilyId), // Thành viên thuộc gia đình có thể truy cập
            new Member("Jane", "Doe", "JD2", inaccessibleFamilyId) // Thành viên thuộc gia đình không thể truy cập
        );
        await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

        // Giả lập ID người dùng hiện tại
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        // Giả lập rằng người dùng hiện tại không phải là quản trị viên
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);

        // Khởi tạo handler với các dependency đã mock
        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);
        var query = new GetMembersQuery(); // Tạo một truy vấn mới

        // Act (Thực thi hành động cần kiểm thử)
        var result = await handler.Handle(query, CancellationToken.None); // Gọi phương thức Handle của handler

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeTrue(); // Đảm bảo truy vấn thành công
        result.Value.Should().NotBeNull(); // Đảm bảo giá trị trả về không rỗng
        result.Value.Should().HaveCount(1); // Đảm bảo chỉ trả về 1 thành viên (từ gia đình có thể truy cập)
        result.Value!.First().FullName.Should().Be("Doe John"); // Kiểm tra tên đầy đủ của thành viên được trả về
    }
}
