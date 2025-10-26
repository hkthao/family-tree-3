using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IFamilyTreeService> _mockFamilyTreeService;

    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _mockFamilyTreeService = new Mock<IFamilyTreeService>();


        _handler = new UpdateMemberCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockFamilyTreeService.Object
        );
    }


    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy thành viên cần cập nhật.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id trả về Id hợp lệ. Thiết lập _mockAuthorizationService để CanManageFamily trả về true.
    ///               Tạo một UpdateMemberCommand với Id của một thành viên không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể cập nhật
    /// một thành viên không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        var nonExistentMemberId = Guid.NewGuid();
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, nonExistentMemberId)
            .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(backend.Application.Common.Constants.ErrorMessages.NotFound, $"Member with ID {nonExistentMemberId}"));
        result.ErrorSource.Should().Be("NotFound");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không có quyền quản lý gia đình mà thành viên thuộc về.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser.Id trả về Id hợp lệ. Thiết lập _mockAuthorizationService để IsAdmin trả về false
    ///               và CanManageFamily trả về false cho bất kỳ FamilyId nào.
    ///    - Act: Gọi phương thức Handle với một UpdateMemberCommand bất kỳ.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng
    /// có quyền quản lý gia đình mới có thể cập nhật thông tin thành viên, bảo vệ dữ liệu gia đình khỏi truy cập trái phép.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(false);

        var command = _fixture.Create<UpdateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(backend.Application.Common.Constants.ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be("Forbidden");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật thành viên thành công
    /// khi người dùng hiện tại là quản trị viên (Admin).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile và một Member. Thiết lập _mockUser.Id trả về Id của UserProfile.
    ///               Thiết lập _mockAuthorizationService để IsAdmin trả về true và CanManageFamily trả về true.
    ///               Thiết lập _mockFamilyTreeService để UpdateFamilyStats trả về Task.CompletedTask.
    ///               Tạo một UpdateMemberCommand với các thông tin cập nhật và Id của Member đã tạo.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên.
    ///              Kiểm tra rằng các thuộc tính của thành viên trong database đã được cập nhật chính xác.
    ///              Xác minh rằng _mockFamilyTreeService.UpdateFamilyStats đã được gọi một lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản trị viên
    /// có thể thực hiện cập nhật thông tin thành viên một cách thành công và các thay đổi được lưu trữ chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateMemberSuccessfully_WhenAdminUser()
    {
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();
        var existingMember = _fixture.Create<Member>();
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>())).Returns(true);
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = _fixture.Build<UpdateMemberCommand>()
                               .With(c => c.Id, existingMember.Id)
                               .With(c => c.FirstName, "UpdatedFirstName")
                               .With(c => c.LastName, "UpdatedLastName")
                               .With(c => c.Nickname, "UpdatedNickname")
                               .With(c => c.Gender, "Female")
                               .With(c => c.DateOfBirth, new DateTime(1990, 1, 1))
                               .With(c => c.DateOfDeath, new DateTime(2050, 1, 1))
                               .With(c => c.PlaceOfBirth, "UpdatedPlaceOfBirth")
                               .With(c => c.PlaceOfDeath, "UpdatedPlaceOfDeath")
                               .With(c => c.Occupation, "UpdatedOccupation")
                               .With(c => c.Biography, "UpdatedBiography")
                               .With(c => c.FamilyId, existingMember.FamilyId) // Keep same family ID
                               .With(c => c.IsRoot, true)
                               .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingMember.Id);

        var updatedMember = await _context.Members.FindAsync(existingMember.Id);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.Nickname.Should().Be(command.Nickname);
        updatedMember.Gender.Should().Be(command.Gender);
        updatedMember.DateOfBirth.Should().Be(command.DateOfBirth);
        updatedMember.DateOfDeath.Should().Be(command.DateOfDeath);
        updatedMember.PlaceOfBirth.Should().Be(command.PlaceOfBirth);
        updatedMember.PlaceOfDeath.Should().Be(command.PlaceOfDeath);
        updatedMember.Occupation.Should().Be(command.Occupation);
        updatedMember.Biography.Should().Be(command.Biography);
        updatedMember.FamilyId.Should().Be(command.FamilyId);
        updatedMember.IsRoot.Should().Be(command.IsRoot);

        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingMember.FamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật thành viên thành công
    /// khi người dùng hiện tại có quyền quản lý gia đình (Manager).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile và một Member. Thiết lập _mockUser.Id trả về Id của UserProfile.
    ///               Thiết lập _mockAuthorizationService để IsAdmin trả về false và CanManageFamily trả về true
    ///               cho FamilyId của thành viên. Thêm FamilyUser với vai trò Manager.
    ///               Thiết lập _mockFamilyTreeService để UpdateFamilyStats trả về Task.CompletedTask.
    ///               Tạo một UpdateMemberCommand với các thông tin cập nhật và Id của Member đã tạo.
    ///    - Act: Gọi phương thức Handle của handler với command đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và Value là Id của thành viên.
    ///              Kiểm tra rằng các thuộc tính của thành viên trong database đã được cập nhật chính xác.
    ///              Xác minh rằng _mockFamilyTreeService.UpdateFamilyStats đã được gọi một lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng có vai trò quản lý gia đình
    /// có thể thực hiện cập nhật thông tin thành viên một cách thành công và các thay đổi được lưu trữ chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateMemberSuccessfully_WhenManagerUser()
    {
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();
        var existingMember = _fixture.Create<Member>();
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(existingMember.FamilyId)).Returns(true);
        _context.FamilyUsers.Add(new FamilyUser
        {
            FamilyId = existingMember.FamilyId,
            UserProfileId = userProfile.Id,
            Role = FamilyRole.Manager
        });
        await _context.SaveChangesAsync();
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = _fixture.Build<UpdateMemberCommand>()
                               .With(c => c.Id, existingMember.Id)
                               .With(c => c.FirstName, "UpdatedFirstNameByManager")
                               .With(c => c.LastName, "UpdatedLastNameByManager")
                               .With(c => c.Nickname, "Nick")
                               .With(c => c.Gender, "Male")
                               .With(c => c.DateOfBirth, new DateTime(1985, 5, 10))
                               .With(c => c.DateOfDeath, (DateTime?)null)
                               .With(c => c.PlaceOfBirth, "City")
                               .With(c => c.PlaceOfDeath, (string?)null)
                               .With(c => c.Occupation, "Job")
                               .With(c => c.Biography, "Short bio.")
                               .With(c => c.FamilyId, existingMember.FamilyId) // Keep same family ID
                               .With(c => c.IsRoot, false)
                               .With(c => c.AvatarUrl, (string?)null)
                               .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingMember.Id);

        var updatedMember = await _context.Members.FindAsync(existingMember.Id);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.Nickname.Should().Be(command.Nickname);
        updatedMember.Gender.Should().Be(command.Gender);
        updatedMember.DateOfBirth.Should().Be(command.DateOfBirth);
        updatedMember.DateOfDeath.Should().Be(command.DateOfDeath);
        updatedMember.PlaceOfBirth.Should().Be(command.PlaceOfBirth);
        updatedMember.PlaceOfDeath.Should().Be(command.PlaceOfDeath);
        updatedMember.Occupation.Should().Be(command.Occupation);
        updatedMember.Biography.Should().Be(command.Biography);
        updatedMember.FamilyId.Should().Be(command.FamilyId);
        updatedMember.IsRoot.Should().Be(command.IsRoot);

        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingMember.FamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
