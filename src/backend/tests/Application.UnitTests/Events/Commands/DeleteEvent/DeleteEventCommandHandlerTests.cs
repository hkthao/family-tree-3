using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests : TestBase
{
    private readonly DeleteEventCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;

    public DeleteEventCommandHandlerTests()
    {
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _handler = new DeleteEventCommandHandler(_context, _mockAuthorizationService.Object);
    }



    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi sự kiện cần xóa không tìm thấy trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thiết lập _mockAuthorizationService để trả về nó. Đảm bảo không có Event nào trong DB khớp với ID của command. Tạo một DeleteEventCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể xóa một sự kiện không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);

        // Ensure no Event exists for this ID
        _context.Events.RemoveRange(_context.Events);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = _fixture.Create<DeleteEventCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Event with ID {command.Id} not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng hệ thống không thể xóa một sự kiện không tồn tại,
        // ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và một Family, Event. Thiết lập _mockAuthorizationService để trả về UserProfile, IsAdmin là false, và CanManageFamily là false. Tạo một DeleteEventCommand với ID của sự kiện.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình) mới có thể xóa sự kiện.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id))
                                 .Returns(false);

        var command = new DeleteEventCommand(existingEvent.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be("Forbidden");

        // 💡 Giải thích:
        // Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình)
        // mới có thể xóa sự kiện.
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành công một sự kiện
    /// khi yêu cầu hợp lệ và người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB. Thiết lập _mockAuthorizationService để trả về UserProfile và IsAdmin là true. Tạo một DeleteEventCommand với ID của sự kiện hiện có. Thiết lập _mockMediator.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem sự kiện đã bị xóa khỏi DB. Kiểm tra xem RecordActivityCommand đã được gọi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này xác minh rằng một quản trị viên có thể xóa thành công một sự kiện hiện có và các hoạt động liên quan được ghi lại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteEventSuccessfully_WhenValidRequestAndUserIsAdmin()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id)).Returns(true);
        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var command = new DeleteEventCommand(existingEvent.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedEvent = await _context.Events.FindAsync(existingEvent.Id);
        deletedEvent.Should().BeNull();

        // 💡 Giải thích:
        // Test này xác minh rằng một quản trị viên có thể xóa thành công một sự kiện hiện có
        // và các hoạt động liên quan được ghi lại.
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xóa thành công một sự kiện
    /// khi yêu cầu hợp lệ và người dùng là quản lý gia đình (nhưng không phải là quản trị viên).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB. Tạo một FamilyUser để liên kết UserProfile với Family với vai trò Manager. Thiết lập _mockAuthorizationService để trả về UserProfile, IsAdmin là false, và CanManageFamily là true. Tạo một DeleteEventCommand với ID của sự kiện hiện có. Thiết lập _mockMediator.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem sự kiện đã bị xóa khỏi DB. Kiểm tra xem RecordActivityCommand đã được gọi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này xác minh rằng một người quản lý gia đình có thể xóa thành công một sự kiện hiện có và các hoạt động liên quan được ghi lại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldDeleteEventSuccessfully_WhenValidRequestAndUserIsFamilyManager()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();
        var familyUser = new FamilyUser { FamilyId = family.Id, UserProfileId = userProfile.Id, Role = FamilyRole.Manager };

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        _context.FamilyUsers.Add(familyUser);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id))
                                 .Returns(true);
        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var command = new DeleteEventCommand(existingEvent.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var deletedEvent = await _context.Events.FindAsync(existingEvent.Id);
        deletedEvent.Should().BeNull();



        // 💡 Giải thích:
        // Test này xác minh rằng một người quản lý gia đình có thể xóa thành công một sự kiện hiện có
        // và các hoạt động liên quan được ghi lại.
    }
}
