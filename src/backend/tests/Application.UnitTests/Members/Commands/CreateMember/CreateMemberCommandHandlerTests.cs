using AutoFixture;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests : TestBase
{
    private readonly CreateMemberCommandHandler _handler;

    public CreateMemberCommandHandlerTests()
    {
        _handler = new CreateMemberCommandHandler(
            _context,
            _mockAuthorizationService.Object
        );
    }



    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về false
    ///               và CanManageFamily trả về false cho bất kỳ FamilyId nào.
    ///    - Act: Gọi phương thức Handle của handler với một CreateMemberCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng
    /// có quyền quản lý gia đình mới có thể tạo thành viên mới, bảo vệ dữ liệu gia đình khỏi truy cập trái phép.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(false);

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(backend.Application.Common.Constants.ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo thành viên thành công
    /// khi người dùng hiện tại là quản trị viên (Admin).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về true.
    ///               Tạo một CreateMemberCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên.
    ///              Kiểm tra rằng thành viên đã được thêm vào context và số lượng thành viên là 1.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản trị viên
    /// có thể tạo thành viên mới một cách thành công mà không cần kiểm tra quyền quản lý gia đình cụ thể.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateMemberSuccessfully_WhenAdminUser()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Should return the ID of the created member
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName);
        _context.Members.Count().Should().Be(1);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo thành viên thành công
    /// khi người dùng hiện tại có quyền quản lý gia đình (Manager).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về false
    ///               và CanManageFamily trả về true cho FamilyId của thành viên. Tạo một CreateMemberCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên.
    ///              Kiểm tra rằng thành viên đã được thêm vào context và số lượng thành viên là 1.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản lý gia đình
    /// có thể tạo thành viên mới một cách thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateMemberSuccessfully_WhenManagerUser()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var command = _fixture.Create<CreateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Should return the ID of the created member
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName);
        _context.Members.Count().Should().Be(1);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng thành viên mới được đặt làm gốc
    /// khi IsRoot là true và chưa có thành viên gốc nào tồn tại trong gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về true.
    ///               Đảm bảo không có thành viên gốc nào trong context cho FamilyId cụ thể.
    ///               Tạo một CreateMemberCommand với IsRoot = true.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên.
    ///              Kiểm tra rằng thành viên được thêm vào có thuộc tính IsRoot = true.
    ///              Kiểm tra rằng số lượng thành viên trong context là 1.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng khi một thành viên mới
    /// được tạo với cờ IsRoot là true và không có thành viên gốc nào khác trong gia đình,
    /// thành viên này sẽ được đánh dấu là gốc của gia đình đó.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSetNewMemberAsRoot_WhenIsRootIsTrueAndNoExistingRoot()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var command = _fixture.Build<CreateMemberCommand>()
            .With(c => c.IsRoot, true)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Should return the ID of the created member
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName && m.IsRoot == true);
        _context.Members.Count().Should().Be(1);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng khi một thành viên mới được đặt làm gốc (IsRoot = true)
    /// và đã có một thành viên gốc khác tồn tại trong cùng gia đình, thì thành viên gốc cũ
    /// sẽ được cập nhật IsRoot = false và thành viên mới sẽ được đặt làm gốc.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về true.
    ///               Tạo và thêm một thành viên gốc hiện có vào context.
    ///               Tạo một CreateMemberCommand với IsRoot = true và cùng FamilyId với thành viên gốc hiện có.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên mới.
    ///              Kiểm tra rằng thành viên gốc cũ đã được cập nhật IsRoot = false.
    ///              Kiểm tra rằng thành viên mới được thêm vào có thuộc tính IsRoot = true.
    ///              Kiểm tra rằng tổng số thành viên trong context là 2.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ có một thành viên
    /// duy nhất có thể là gốc trong một gia đình tại một thời điểm. Khi một thành viên mới được
    /// chỉ định làm gốc, thành viên gốc hiện có sẽ tự động bị hủy đặt gốc.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateExistingRoot_WhenIsRootIsTrueAndExistingRootExists()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Code = "FAM001", Name = "Test Family" };
        _context.Families.Add(family);

        var existingRootId = Guid.NewGuid();
        var existingRoot = new Member
        {
            Id = existingRootId,
            FamilyId = familyId,
            IsRoot = true,
            FirstName = "Existing",
            LastName = "Root",
            Code = "ER001"
        };

        _context.Members.Add(existingRoot);
        await _context.SaveChangesAsync();

        var command = new CreateMemberCommand
        {
            FamilyId = familyId,
            IsRoot = true,
            FirstName = "New",
            LastName = "Member",
            Code = "NM001"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Should return the ID of the created member

        // Re-fetch the existing root to ensure its state is updated
        var updatedExistingRoot = await _context.Members.FindAsync(existingRoot.Id);
        updatedExistingRoot.Should().NotBeNull();
        updatedExistingRoot!.IsRoot.Should().BeFalse();

        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName && m.IsRoot == true);
        _context.Members.Count().Should().Be(2);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo một mã duy nhất cho thành viên
    /// khi trường Code trong CreateMemberCommand là null hoặc rỗng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về true.
    ///               Tạo một CreateMemberCommand với Code là null.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công.
    ///              Kiểm tra rằng thành viên đã được thêm vào context và trường Code không rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống tự động
    /// tạo một mã duy nhất cho thành viên khi người dùng không cung cấp, duy trì tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateUniqueCode_WhenCodeIsNull()
    {
        _mockUser.Setup(u => u.Id).Returns(Guid.NewGuid());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var command = _fixture.Build<CreateMemberCommand>()
            .With(c => c.Code, (string)null!)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _context.Members.Should().Contain(m => m.FirstName == command.FirstName && m.LastName == command.LastName && !string.IsNullOrEmpty(m.Code));
        _context.Members.Count().Should().Be(1);
    }
}
