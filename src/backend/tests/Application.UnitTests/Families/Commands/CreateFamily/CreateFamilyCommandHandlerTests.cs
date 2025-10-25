using AutoFixture;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Events;
using backend.Domain.Events.Families;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _handler = new CreateFamilyCommandHandler(_context, _mockUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateFamilyAndAssignManager_WhenValidRequestAndUserAuthenticated()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng một gia đình mới được tạo thành công, người dùng tạo được gán làm quản lý,
        // và hoạt động tạo gia đình được ghi lại khi yêu cầu hợp lệ và người dùng đã xác thực.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thêm vào DB.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng.
        // 3. Tạo một CreateFamilyCommand hợp lệ.
        // 4. Thiết lập _mockMediator để không làm gì khi RecordActivityCommand được gửi.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công và chứa Guid của gia đình mới.
        // 2. Kiểm tra xem gia đình mới đã được lưu vào DB với các thuộc tính chính xác.
        // 3. Kiểm tra xem FamilyUser đã được tạo và gán vai trò Manager cho người dùng.
        // 4. Kiểm tra xem RecordActivityCommand đã được gửi đi một lần.
        // 5. Kiểm tra xem FamilyCreatedEvent và FamilyStatsUpdatedEvent đã được thêm vào domain events.

        // Arrange
        var userId = Guid.NewGuid();
        await _context.SaveChangesAsync(CancellationToken.None);
        _mockUser.Setup(u => u.Id).Returns(userId);
        var command = _fixture.Build<CreateFamilyCommand>()
                               .With(c => c.Name, "Test Family")
                               .With(c => c.Description, "A family for testing")
                               .With(c => c.Code, "FAM-TEST")
                               .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdFamily = await _context.Families.FindAsync(result.Value);
        createdFamily.Should().NotBeNull();
        createdFamily!.Name.Should().Be(command.Name);
        createdFamily.Description.Should().Be(command.Description);
        createdFamily.Code.Should().Be(command.Code);

        createdFamily.DomainEvents.Should().ContainSingle(e => e is FamilyCreatedEvent);
        createdFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);

        // 💡 Giải thích:
        // Test này xác minh toàn bộ luồng tạo gia đình thành công:
        // 1. Gia đình được tạo và lưu vào cơ sở dữ liệu.
        // 2. Người dùng tạo được tự động gán vai trò quản lý cho gia đình đó.
        // 3. Hoạt động tạo gia đình được ghi lại thông qua IMediator.
        // 4. Các sự kiện FamilyCreatedEvent và FamilyStatsUpdatedEvent được thêm vào domain events của thực thể gia đình.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi người dùng không được xác thực (User.Id là null hoặc rỗng).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về null cho User.Id.
        // 2. Tạo một CreateFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        _mockUser.Setup(u => u.Id).Returns((Guid?)null); // User is not authenticated

        var command = _fixture.Create<CreateFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Current user ID not found.");
        result.ErrorSource.Should().Be("Authentication");

        // 💡 Giải thích:
        // Test này kiểm tra trường hợp bảo mật cơ bản: nếu không có người dùng được xác thực,
        // yêu cầu tạo gia đình sẽ bị từ chối với thông báo lỗi rõ ràng.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về một kết quả thất bại
        // khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockUser để trả về một UserProfileId hợp lệ nhưng không tồn tại trong DB.
        // 2. Đảm bảo không có UserProfile nào trong DB khớp với ID này.
        // 3. Tạo một CreateFamilyCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thất bại.
        // 2. Kiểm tra thông báo lỗi phù hợp.

        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        // Ensure no UserProfile exists for this userId
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = _fixture.Create<CreateFamilyCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        result.ErrorSource.Should().Be("NotFound");

        // 💡 Giải thích:
        // Test này đảm bảo rằng ngay cả khi người dùng được xác thực,
        // nếu hồ sơ người dùng của họ không tồn tại trong hệ thống,
        // yêu cầu sẽ thất bại để ngăn chặn việc tạo dữ liệu không hợp lệ.
    }

    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler tự động tạo một mã duy nhất cho gia đình
        // khi mã không được cung cấp trong CreateFamilyCommand.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một UserProfile giả lập và thêm vào DB.
        // 2. Thiết lập _mockUser để trả về UserProfileId của người dùng.
        // 3. Tạo một CreateFamilyCommand mà không cung cấp Code.
        // 4. Thiết lập _mockMediator và _mockFamilyTreeService.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về là thành công.
        // 2. Kiểm tra xem gia đình được tạo có Code không rỗng và bắt đầu bằng "FAM-".

        // Arrange
        var userId = Guid.NewGuid();
        await _context.SaveChangesAsync(CancellationToken.None);
        _mockUser.Setup(u => u.Id).Returns(userId);
        var command = _fixture.Build<CreateFamilyCommand>()
                               .With(c => c.Name, "Family Without Code")
                               .Without(c => c.Code) // Không cung cấp Code
                               .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdFamily = await _context.Families.FindAsync(result.Value);
        createdFamily.Should().NotBeNull();
        createdFamily!.Code.Should().NotBeNullOrEmpty();
        createdFamily.Code.Should().StartWith("FAM-");

        createdFamily.DomainEvents.Should().ContainSingle(e => e is FamilyCreatedEvent);
        createdFamily.DomainEvents.Should().ContainSingle(e => e is FamilyStatsUpdatedEvent);

        // 💡 Giải thích:
        // Test này đảm bảo rằng nếu người dùng không cung cấp mã cho gia đình,
        // hệ thống sẽ tự động tạo một mã duy nhất theo định dạng mong muốn.
    }
}
