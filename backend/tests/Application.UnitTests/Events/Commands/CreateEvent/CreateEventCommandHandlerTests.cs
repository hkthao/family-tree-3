using backend.Application.Events.Commands.CreateEvent;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Application.Common.Exceptions; // Added for ValidationException
using Moq; // Added for Mock
using FluentValidation; // Added for validator usage

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _handler = new CreateEventCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object);
    }

    /// <summary>
    /// Kiểm tra xem sự kiện có được tạo thành công khi tất cả các điều kiện hợp lệ.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một lệnh CreateEventCommand hợp lệ được cung cấp
    /// và người dùng có quyền (là Admin), một sự kiện mới sẽ được tạo và lưu vào cơ sở dữ liệu.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Create_Event()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền để trả về một UserProfile hợp lệ và quyền Admin.
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new backend.Domain.Entities.UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Tạo một lệnh CreateEventCommand hợp lệ.
        var command = new CreateEventCommand
        {
            Name = "Sự kiện Mới",
            Description = "Mô tả sự kiện mới",
        };

        // Act (Thực hiện hành động cần kiểm tra)
        // Gửi lệnh tạo sự kiện thông qua handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo rằng lệnh được thực thi thành công.
        result.IsSuccess.Should().BeTrue();
        var eventId = result.Value;

        // Tìm sự kiện vừa tạo trong cơ sở dữ liệu để xác minh.
        var createdEvent = await _context.Events.FindAsync(eventId);
        // Đảm bảo sự kiện không null và các thuộc tính khớp với dữ liệu đã gửi.
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be("Sự kiện Mới");
        createdEvent.Description.Should().Be("Mô tả sự kiện mới");
    }

    /// <summary>
    /// Kiểm tra xem có ném ra ngoại lệ ValidationException khi tên sự kiện rỗng.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng quy tắc xác thực (validation rule) cho trường 'Name' hoạt động đúng,
    /// tức là không cho phép tạo sự kiện với tên rỗng và ném ra FluentValidation.ValidationException.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Tạo một lệnh CreateEventCommand với tên rỗng.
        var command = new CreateEventCommand
        {
            Name = "",
            Description = "Một sự kiện mới với tên rỗng",
        };

        // Khởi tạo validator trực tiếp để kiểm tra quy tắc xác thực.
        var validator = new CreateEventCommandValidator();

        // Act (Thực hiện hành động cần kiểm tra)
        // Thực thi phương thức ValidateAndThrowAsync của validator.
        Func<Task> act = async () => await validator.ValidateAndThrowAsync(command);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo rằng một FluentValidation.ValidationException được ném ra.
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Kiểm tra xem có bị từ chối truy cập khi người dùng không có quyền tạo sự kiện.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng chỉ những người dùng có quyền (Admin hoặc quản lý gia đình)
    /// mới có thể tạo sự kiện. Nếu người dùng không có quyền, lệnh sẽ trả về kết quả thất bại
    /// với thông báo lỗi truy cập bị từ chối.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        // Giả lập dịch vụ ủy quyền: người dùng không phải Admin và không thể quản lý gia đình.
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new backend.Domain.Entities.UserProfile { Id = Guid.NewGuid() });
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>(), It.IsAny<backend.Domain.Entities.UserProfile>())).Returns(false);

        // Tạo một lệnh CreateEventCommand với FamilyId để kích hoạt kiểm tra CanManageFamily.s
        var command = new CreateEventCommand
        {
            Name = "Sự kiện Không Được Phép",
            Description = "Sự kiện được tạo bởi người dùng không có quyền",
            FamilyId = Guid.NewGuid() // Cung cấp FamilyId để kiểm tra CanManageFamily
        };

        // Act (Thực hiện hành động cần kiểm tra)
        // Gửi lệnh tạo sự kiện thông qua handler.
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo rằng lệnh thực thi thất bại.
        result.IsSuccess.Should().BeFalse();
        // Đảm bảo thông báo lỗi chứa chuỗi "Access denied".
        result.Error.Should().Contain("Access denied. Only family managers or admins can create events.");
    }
}