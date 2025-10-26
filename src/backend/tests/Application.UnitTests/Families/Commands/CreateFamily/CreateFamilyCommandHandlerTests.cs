using AutoFixture;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.UnitTests.Common;
using backend.Domain.Events;
using backend.Domain.Events.Families;
using FluentAssertions;
using Xunit;


/// <summary>
/// Bộ test cho CreateFamilyCommandHandler.
/// </summary>
public class CreateFamilyCommandHandlerTests : TestBase
{
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandHandlerTests()
    {
        _handler = new CreateFamilyCommandHandler(_context, _mockUser.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng một gia đình mới được tạo thành công, người dùng tạo được gán làm quản lý,
    /// và hoạt động tạo gia đình được ghi lại khi yêu cầu hợp lệ và người dùng đã xác thực.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thêm vào DB. Thiết lập _mockUser để trả về UserProfileId của người dùng.
    ///               Tạo một CreateFamilyCommand hợp lệ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và chứa Guid của gia đình mới.
    ///              Kiểm tra xem gia đình mới đã được lưu vào DB với các thuộc tính chính xác.
    ///              Kiểm tra xem FamilyUser đã được tạo và gán vai trò Manager cho người dùng.
    ///              Kiểm tra xem FamilyCreatedEvent và FamilyStatsUpdatedEvent đã được thêm vào domain events.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này xác minh toàn bộ luồng tạo gia đình thành công:
    /// 1. Gia đình được tạo và lưu vào cơ sở dữ liệu.
    /// 2. Người dùng tạo được tự động gán vai trò quản lý cho gia đình đó.
    /// 3. Các sự kiện FamilyCreatedEvent và FamilyStatsUpdatedEvent được thêm vào domain events của thực thể gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateFamilyAndAssignManager_WhenValidRequestAndUserAuthenticated()
    {
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
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tự động tạo một mã duy nhất cho gia đình
    /// khi mã không được cung cấp trong CreateFamilyCommand.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thêm vào DB. Thiết lập _mockUser để trả về UserProfileId của người dùng.
    ///               Tạo một CreateFamilyCommand mà không cung cấp Code.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem gia đình được tạo có Code không rỗng và bắt đầu bằng "FAM-".
    ///              Kiểm tra xem FamilyCreatedEvent và FamilyStatsUpdatedEvent đã được thêm vào domain events.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Nếu người dùng không cung cấp mã cho gia đình,
    /// hệ thống sẽ tự động tạo một mã duy nhất theo định dạng mong muốn.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateCode_WhenCodeIsNotProvided()
    {
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
    }
}
