using backend.Application.UnitTests.Common;
using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Common.Exceptions;
using backend.Domain.Entities;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandlerTests : TestBase
{
    private readonly DeleteEventCommandHandler _handler;

    public DeleteEventCommandHandlerTests()
    {
        _handler = new DeleteEventCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object);
    }

    /// <summary>
    /// Kiểm tra xem một sự kiện có được xóa thành công khi người dùng có quyền.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh DeleteEventCommand hợp lệ được thực thi
    /// bởi một người dùng có quyền (Admin), sự kiện tương ứng sẽ bị xóa khỏi cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Delete_Event_Successfully()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền để trả về một UserProfile hợp lệ và quyền Admin.
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Thêm một sự kiện vào cơ sở dữ liệu để xóa.
        var eventToDelete = new Event { Id = Guid.NewGuid(), Name = "Sự kiện để xóa", FamilyId = Guid.NewGuid() };
        _context.Events.Add(eventToDelete);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa sự kiện.
        var command = new DeleteEventCommand(eventToDelete.Id);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thành công.
        result.IsSuccess.Should().BeTrue();
        // Đảm bảo sự kiện không còn tồn tại trong cơ sở dữ liệu.
        var deletedEvent = await _context.Events.FindAsync(eventToDelete.Id);
        deletedEvent.Should().BeNull();
        // Đảm bảo RecordActivityCommand được gửi đi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi cố gắng xóa một sự kiện không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh DeleteEventCommand được thực thi với một ID sự kiện không tồn tại,
    /// hệ thống sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EventNotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền để trả về một UserProfile hợp lệ và quyền Admin.
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Tạo lệnh xóa sự kiện với một ID không tồn tại.
        var nonExistentEventId = Guid.NewGuid();
        var command = new DeleteEventCommand(nonExistentEventId);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thất bại.
        result.IsSuccess.Should().BeFalse();
        // Đảm bảo thông báo lỗi chứa chuỗi "not found".
        result.Error.Should().Contain($"Event with ID {nonExistentEventId} not found.");
        // Đảm bảo RecordActivityCommand không được gửi đi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền xóa sự kiện.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền (Admin hoặc quản lý gia đình)
    /// mới có thể xóa sự kiện. Nếu người dùng không có quyền, lệnh sẽ trả về kết quả thất bại
    /// với thông báo lỗi truy cập bị từ chối.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIsNotAuthorized()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền: người dùng không phải Admin và không thể quản lý gia đình.
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>(), It.IsAny<UserProfile>())).Returns(false);

        // Thêm một sự kiện vào cơ sở dữ liệu để xóa, với FamilyId.
        var familyId = Guid.NewGuid();
        var eventToDelete = new Event { Id = Guid.NewGuid(), Name = "Sự kiện không được phép xóa", FamilyId = familyId };
        _context.Events.Add(eventToDelete);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh xóa sự kiện.
        var command = new DeleteEventCommand(eventToDelete.Id);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thất bại.
        result.IsSuccess.Should().BeFalse();
        // Đảm bảo thông báo lỗi chứa chuỗi "Access denied".
        result.Error.Should().Contain("Access denied. Only family managers or admins can delete events.");
        // Đảm bảo RecordActivityCommand không được gửi đi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
