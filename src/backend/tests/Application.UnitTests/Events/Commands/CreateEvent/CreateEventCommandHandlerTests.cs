using AutoFixture;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvent;

public class CreateEventCommandHandlerTests : TestBase
{
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _handler = new CreateEventCommandHandler(_context, _mockAuthorizationService.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi AccessDenied khi người dùng không có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand với FamilyId bất kỳ. Thiết lập _mockAuthorizationService.CanManageFamily trả về false.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thất bại, với thông báo lỗi là ErrorMessages.AccessDenied và ErrorSource là ErrorSources.Forbidden.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Chỉ quản trị viên hoặc người quản lý gia đình mới có thể tạo sự kiện.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        // Arrange
        var command = _fixture.Create<CreateEventCommand>();
        _mockAuthorizationService.Setup(s => s.CanManageFamily(command.FamilyId!.Value)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng một sự kiện được tạo thành công khi lệnh hợp lệ và người dùng có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand hợp lệ. Thiết lập _mockAuthorizationService.CanManageFamily trả về true.
    ///               Thêm các thành viên liên quan vào _context.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. Sự kiện được thêm vào _context.Events. SaveChangesAsync được gọi.
    ///              Một EventCreatedEvent được thêm vào các domain event của sự kiện.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải tạo sự kiện và lưu vào DB khi tất cả các điều kiện hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateEventSuccessfully_WhenValidCommandAndUserCanManageFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member1", LastName = "Test", Code = "M1" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member2", LastName = "Test", Code = "M2" };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var command = _fixture.Build<CreateEventCommand>()
            .With(c => c.FamilyId, familyId)
            .With(c => c.RelatedMembers, new List<Guid> { member1.Id, member2.Id })
            .Create();

        _mockAuthorizationService.Setup(s => s.CanManageFamily(command.FamilyId!.Value)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _context.Events.Should().HaveCount(1);
        var createdEvent = _context.Events.First();
        createdEvent.Name.Should().Be(command.Name);
        createdEvent.FamilyId.Should().Be(command.FamilyId);
        createdEvent.EventMembers.Should().HaveCount(2);
        createdEvent.DomainEvents.Should().ContainSingle(e => e.GetType() == typeof(Domain.Events.Events.EventCreatedEvent));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo một mã duy nhất cho sự kiện khi Code không được cung cấp trong lệnh.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một CreateEventCommand hợp lệ nhưng không cung cấp giá trị cho thuộc tính Code. Thiết lập _mockAuthorizationService.CanManageFamily trả về true.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra kết quả trả về là thành công. Sự kiện được tạo có thuộc tính Code không rỗng và bắt đầu bằng "EVT-".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải tự động tạo mã khi không được cung cấp.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateEventWithGeneratedCode_WhenCodeIsNotProvided()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = "Member1", LastName = "Test", Code = "M1" };
        _context.Members.Add(member1);
        await _context.SaveChangesAsync();

        var command = _fixture.Build<CreateEventCommand>()
            .With(c => c.FamilyId, familyId)
            .With(c => c.RelatedMembers, new List<Guid> { member1.Id })
            .Without(c => c.Code) // Ensure Code is not provided
            .Create();

        _mockAuthorizationService.Setup(s => s.CanManageFamily(command.FamilyId!.Value)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _context.Events.Should().HaveCount(1);
        var createdEvent = _context.Events.First();
        createdEvent.Code.Should().NotBeNullOrEmpty();
        createdEvent.Code.Should().StartWith("EVT-");
    }
}

