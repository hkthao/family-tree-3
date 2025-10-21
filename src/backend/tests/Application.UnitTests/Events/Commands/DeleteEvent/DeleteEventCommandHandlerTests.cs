using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.UnitTests.Common;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests : TestBase
{
    private readonly DeleteEventCommandHandler _handler;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;

    public DeleteEventCommandHandlerTests()
    {
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();

        _handler = new DeleteEventCommandHandler(_context, _mockAuthorizationService.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockAuthorizationService để trả về null cho GetCurrentUserProfileAsync.
        // 2. Tạo một DeleteEventCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((UserProfile)null!); // UserProfile not found

        var command = _fixture.Create<DeleteEventCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu hồ sơ người dùng không tồn tại trong hệ thống,
        // yêu cầu xóa sự kiện sẽ thất bại để ngăn chặn việc thao tác dữ liệu không hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi sự kiện cần xóa không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thiết lập _mockAuthorizationService để trả về nó.
        // 2. Đảm bảo không có Event nào trong DB khớp với ID của command.
        // 3. Tạo một DeleteEventCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);

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

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi người dùng không phải là quản trị viên và không có quyền quản lý gia đình.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và một Family, Event.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile, IsAdmin là false,
        //    và CanManageFamily là false.
        // 3. Tạo một DeleteEventCommand với ID của sự kiện.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id, userProfile))
                                 .Returns(false);

        var command = new DeleteEventCommand(existingEvent.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Access denied. Only family managers or admins can delete events.");
        result.ErrorSource.Should().Be("Forbidden");

        // 💡 Giải thích:
        // Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình)
        // mới có thể xóa sự kiện.
    }

    [Fact]
    public async Task Handle_ShouldDeleteEventSuccessfully_WhenValidRequestAndUserIsAdmin()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler xóa thành công một sự kiện
        // khi yêu cầu hợp lệ và người dùng là quản trị viên.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB.
        // 2. Thiết lập _mockAuthorizationService để trả về UserProfile và IsAdmin là true.
        // 3. Tạo một DeleteEventCommand với ID của sự kiện hiện có.
        // 4. Thiết lập _mockMediator.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem sự kiện đã bị xóa khỏi DB.
        // 3. Kiểm tra xem RecordActivityCommand đã được gọi.

        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
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

        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        // 💡 Giải thích:
        // Test này xác minh rằng một quản trị viên có thể xóa thành công một sự kiện hiện có
        // và các hoạt động liên quan được ghi lại.
    }

    [Fact]
    public async Task Handle_ShouldDeleteEventSuccessfully_WhenValidRequestAndUserIsFamilyManager()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler xóa thành công một sự kiện
        // khi yêu cầu hợp lệ và người dùng là quản lý gia đình (nhưng không phải là quản trị viên).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB.
        // 2. Tạo một FamilyUser để liên kết UserProfile với Family với vai trò Manager.
        // 3. Thiết lập _mockAuthorizationService để trả về UserProfile, IsAdmin là false,
        //    và CanManageFamily là true.
        // 4. Tạo một DeleteEventCommand với ID của sự kiện hiện có.
        // 5. Thiết lập _mockMediator.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem sự kiện đã bị xóa khỏi DB.
        // 3. Kiểm tra xem RecordActivityCommand đã được gọi.

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

        _mockAuthorizationService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id, userProfile))
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

        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        // 💡 Giải thích:
        // Test này xác minh rằng một người quản lý gia đình có thể xóa thành công một sự kiện hiện có
        // và các hoạt động liên quan được ghi lại.
    }
}