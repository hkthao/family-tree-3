using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : TestBase
{
    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _handler = new UpdateEventCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object);
    }

    /// <summary>
    /// Kiểm tra xem một sự kiện có được cập nhật thành công khi tất cả các điều kiện hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh UpdateEventCommand hợp lệ được cung cấp
    /// và người dùng có quyền (là Admin), một sự kiện hiện có sẽ được cập nhật trong cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Update_Event()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền để trả về một UserProfile hợp lệ và quyền Admin.
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Thêm một sự kiện vào cơ sở dữ liệu để cập nhật.
        var existingEvent = new Event { Id = Guid.NewGuid(), Name = "Tên Cũ", Description = "Mô tả cũ", FamilyId = Guid.NewGuid(), Code = "EVT" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper() };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh cập nhật sự kiện.
        var command = new UpdateEventCommand
        {
            Id = existingEvent.Id,
            Name = "Tên Mới",
            Description = "Mô tả mới",
            FamilyId = existingEvent.FamilyId
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thành công.
        result.IsSuccess.Should().BeTrue();
        // Tìm sự kiện đã cập nhật trong cơ sở dữ liệu để xác minh.
        var updatedEvent = await _context.Events.FindAsync(existingEvent.Id);
        // Đảm bảo sự kiện không null và các thuộc tính đã được cập nhật.
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be("Tên Mới");
        updatedEvent.Description.Should().Be("Mô tả mới");
        // Đảm bảo RecordActivityCommand được gửi đi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi cố gắng cập nhật một sự kiện không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh UpdateEventCommand được thực thi với một ID sự kiện không tồn tại,
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

        // Tạo lệnh cập nhật sự kiện với một ID không tồn tại.
        var nonExistentEventId = Guid.NewGuid();
        var command = new UpdateEventCommand
        {
            Id = nonExistentEventId,
            Name = "Tên Mới",
            Description = "Mô tả mới",
            FamilyId = Guid.NewGuid(),
            Code = "EVT" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper()
        };

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
    /// Kiểm tra xem có trả về lỗi khi người dùng không có quyền cập nhật sự kiện.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền (Admin hoặc quản lý gia đình)
    /// mới có thể cập nhật sự kiện. Nếu người dùng không có quyền, lệnh sẽ trả về kết quả thất bại
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

        // Thêm một sự kiện vào cơ sở dữ liệu để cập nhật, với FamilyId.
        var familyId = Guid.NewGuid();
        var existingEvent = new Event { Id = Guid.NewGuid(), Name = "Sự kiện không được phép cập nhật", FamilyId = familyId, Code = "EVT" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper() };
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo lệnh cập nhật sự kiện.
        var command = new UpdateEventCommand
        {
            Id = existingEvent.Id,
            Name = "Tên Mới",
            Description = "Mô tả mới",
            FamilyId = existingEvent.FamilyId,
            Code = existingEvent.Code
        };

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo lệnh thực thi thất bại.
        result.IsSuccess.Should().BeFalse();
        // Đảm bảo thông báo lỗi chứa chuỗi "Access denied".
        result.Error.Should().Contain("Access denied. Only family managers or admins can update events.");
        // Đảm bảo RecordActivityCommand không được gửi đi.
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.UserActivities.Commands.RecordActivity.RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
