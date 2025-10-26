using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly CreateEventCommandHandler _handler;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;

    public CreateEventCommandHandlerTests()
    {
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();


        _handler = new CreateEventCommandHandler(_context, _mockAuthorizationService.Object, _mockUser.Object);
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
        // 2. Tạo một CreateEventCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns((Guid?)null); // Simulate UserProfile not found

        var command = _fixture.Create<CreateEventCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu hồ sơ người dùng không tồn tại trong hệ thống,
        // yêu cầu tạo sự kiện sẽ thất bại để ngăn chặn việc thao tác dữ liệu không hợp lệ.
    }
}
