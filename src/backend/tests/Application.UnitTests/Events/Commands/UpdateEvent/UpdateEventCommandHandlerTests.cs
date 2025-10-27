using backend.Application.Common.Constants;
using AutoFixture;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandlerTests : TestBase
{
    private readonly UpdateEventCommandHandler _handler;
    private readonly Mock<IMediator> _mockMediator;

    public UpdateEventCommandHandlerTests()
    {
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _handler = new UpdateEventCommandHandler(_context, _mockAuthorizationService.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi UserProfile của người dùng được xác thực không tìm thấy trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser để trả về null cho Id, dẫn đến _mockAuthorizationService.CanManageFamily trả về false. Tạo một UpdateEventCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp (AccessDenied).
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng nếu không có người dùng được xác thực, yêu cầu cập nhật sự kiện sẽ bị từ chối quyền truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        _mockUser.Setup(u => u.Id).Returns((Guid?)null); // Simulate no authenticated user
        // Since there's no authenticated user, CanManageFamily will return false, leading to AccessDenied
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>())).Returns(false);

        var command = _fixture.Create<UpdateEventCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi sự kiện cần cập nhật không tìm thấy trong cơ sở dữ liệu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để CanManageFamily trả về true. Đảm bảo không có Event nào trong DB khớp với ID của command. Tạo một UpdateEventCommand bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể cập nhật một sự kiện không tồn tại, ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(It.IsAny<Guid>())).Returns(true);

        // Ensure no Event exists for this ID
        _context.Events.RemoveRange(_context.Events);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = _fixture.Create<UpdateEventCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.EventNotFound, command.Id));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi người dùng không có quyền quản lý gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và một Family, Event. Thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để CanManageFamily là false. Tạo một UpdateEventCommand với ID của sự kiện.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chỉ những người dùng có quyền (quản trị viên hoặc người quản lý gia đình) mới có thể cập nhật sự kiện.
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
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id))
                                 .Returns(false);

        var command = _fixture.Build<UpdateEventCommand>()
                            .With(c => c.Id, existingEvent.Id)
                            .With(c => c.FamilyId, family.Id)
                            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật thành công một sự kiện
    /// khi yêu cầu hợp lệ và người dùng là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    /// Arrange:
    /// 1. Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB.
    /// 2. Thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để IsAdmin là true và CanManageFamily là true.
    /// 3. Tạo một UpdateEventCommand với các giá trị mới.
    /// Act:
    /// 1. Gọi phương thức Handle của handler.
    /// Assert:
    /// 1. Kiểm tra xem kết quả trả về là thành công.
    /// 2. Kiểm tra xem thông tin sự kiện trong DB đã được cập nhật chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateEventSuccessfully_WhenValidRequestAndUserIsAdmin()
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

        var command = _fixture.Build<UpdateEventCommand>()
                            .With(c => c.Id, existingEvent.Id)
                            .With(c => c.Name, "Updated Event Name")
                            .With(c => c.Description, "Updated Description")
                            .With(c => c.Location, "Updated Location")
                            .With(c => c.FamilyId, family.Id)
                            .With(c => c.StartDate, DateTime.Now.AddDays(1))
                            .With(c => c.EndDate, DateTime.Now.AddDays(2))
                            .With(c => c.Color, "#000000")
                            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedEvent = await _context.Events.FindAsync(existingEvent.Id);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.Description.Should().Be(command.Description);
        updatedEvent.Location.Should().Be(command.Location);
        updatedEvent.FamilyId.Should().Be(command.FamilyId);
        updatedEvent.StartDate.Should().Be(command.StartDate);
        updatedEvent.EndDate.Should().Be(command.EndDate);
        updatedEvent.Color.Should().Be(command.Color);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật thành công một sự kiện
    /// khi yêu cầu hợp lệ và người dùng là quản lý gia đình (nhưng không phải là quản trị viên).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập, Family và Event hiện có, sau đó thêm vào DB. Tạo một FamilyUser để liên kết UserProfile với Family với vai trò Manager. Thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để IsAdmin là false, và CanManageFamily là true.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem thông tin sự kiện trong DB đã được cập nhật chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này xác minh rằng một người quản lý gia đình có thể cập nhật thành công một sự kiện hiện có.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateEventSuccessfully_WhenValidRequestAndUserIsFamilyManager()
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

        var command = _fixture.Build<UpdateEventCommand>()
                            .With(c => c.Id, existingEvent.Id)
                            .With(c => c.Name, "Updated Event Name by Manager")
                            .With(c => c.Description, "Updated Description by Manager")
                            .With(c => c.Location, "Updated Location by Manager")
                            .With(c => c.FamilyId, family.Id)
                            .With(c => c.StartDate, DateTime.Now.AddDays(1))
                            .With(c => c.EndDate, DateTime.Now.AddDays(2))
                            .With(c => c.Color, "#FF0000")
                            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedEvent = await _context.Events.FindAsync(existingEvent.Id);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.Description.Should().Be(command.Description);
        updatedEvent.Location.Should().Be(command.Location);
        updatedEvent.FamilyId.Should().Be(command.FamilyId);
        updatedEvent.StartDate.Should().Be(command.StartDate);
        updatedEvent.EndDate.Should().Be(command.EndDate);
        updatedEvent.Color.Should().Be(command.Color);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler cập nhật thành công một sự kiện với các thành viên liên quan.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập, Family, Event và Members, sau đó thêm vào DB. Thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để IsAdmin là true và CanManageFamily là true.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem thông tin sự kiện trong DB đã được cập nhật chính xác, bao gồm RelatedMembers.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này xác minh rằng một quản trị viên có thể cập nhật thành công một sự kiện
    /// bao gồm cả việc liên kết các thành viên liên quan.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateEventSuccessfully_WithRelatedMembers()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        var family = _fixture.Create<Family>();
        var existingEvent = _fixture.Build<Event>().With(e => e.FamilyId, family.Id).Create();
        var member1 = _fixture.Build<Member>().With(m => m.FamilyId, family.Id).Create();
        var member2 = _fixture.Build<Member>().With(m => m.FamilyId, family.Id).Create();

        _context.UserProfiles.Add(userProfile);
        _context.Families.Add(family);
        _context.Events.Add(existingEvent);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        _mockAuthorizationService.Setup(s => s.CanManageFamily(family.Id)).Returns(true);

        var command = _fixture.Build<UpdateEventCommand>()
                            .With(c => c.Id, existingEvent.Id)
                            .With(c => c.Name, "Event with Related Members")
                            .With(c => c.FamilyId, family.Id)
                            .With(c => c.RelatedMembers, new List<Guid> { member1.Id, member2.Id })
                            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedEvent = await _context.Events.Include(e => e.EventMembers).FirstOrDefaultAsync(e => e.Id == existingEvent.Id);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be(command.Name);
        updatedEvent.EventMembers.Should().HaveCount(2);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member1.Id);
        updatedEvent.EventMembers.Select(em => em.MemberId).Should().Contain(member2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi một FamilyId không hợp lệ (không tồn tại) được cung cấp trong command.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một UserProfile giả lập và thiết lập _mockUser.Id. Thiết lập _mockAuthorizationService để CanManageFamily trả về false cho FamilyId không tồn tại. Tạo một UpdateEventCommand với một FamilyId không tồn tại.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại. Kiểm tra thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không thể cập nhật sự kiện cho một gia đình không tồn tại,
    /// ngăn chặn các lỗi tham chiếu và đảm bảo tính toàn vẹn dữ liệu.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidFamilyId()
    {
        // Arrange
        var userProfile = _fixture.Create<UserProfile>();
        _mockUser.Setup(u => u.Id).Returns(userProfile.Id);
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false); // Not an admin

        var invalidFamilyId = Guid.NewGuid(); // FamilyId không tồn tại
        _mockAuthorizationService.Setup(s => s.CanManageFamily(invalidFamilyId)).Returns(false); // Không có quyền quản lý gia đình không tồn tại

        var command = _fixture.Build<UpdateEventCommand>()
                            .With(c => c.Id, Guid.NewGuid())
                            .With(c => c.FamilyId, invalidFamilyId)
                            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
