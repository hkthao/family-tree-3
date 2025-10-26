using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.GetUpcomingEvents;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandlerTests : TestBase
{
    private readonly GetUpcomingEventsQueryHandler _handler;

    public GetUpcomingEventsQueryHandlerTests()
    {
        _handler = new GetUpcomingEventsQueryHandler(
            _context,
            _mapper,
            _mockAuthorizationService.Object,
            _mockUser.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một danh sách sự kiện rỗng
    /// khi không có ID người dùng và người dùng không phải là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService để IsAdmin trả về false. Thiết lập _mockUser để Id trả về null.
    ///               Tạo một GetUpcomingEventsQuery bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện là rỗng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống không trả về
    /// bất kỳ sự kiện nào khi không thể xác định người dùng và người dùng không có quyền quản trị.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUserIdAndNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        _mockUser.Setup(u => u.Id).Returns((Guid?)null);
        var query = _fixture.Create<GetUpcomingEventsQuery>();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả các sự kiện
    /// khi người dùng hiện tại là quản trị viên, bỏ qua các bộ lọc quyền truy cập gia đình.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService để IsAdmin trả về true. Thêm một số sự kiện vào DB.
    ///               Tạo một GetUpcomingEventsQuery bất kỳ.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chứa tất cả các sự kiện đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng quản trị viên có thể xem
    /// tất cả các sự kiện trong hệ thống.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllEvents_WhenAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(true);
        var family1 = _fixture.Create<Family>();
        var family2 = _fixture.Create<Family>();
        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.FamilyId, family1.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(1))
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.FamilyId, family2.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(2))
            .Create();
        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUpcomingEventsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Should().Contain(e => e.Id == event1.Id);
        result.Value!.Should().Contain(e => e.Id == event2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo ID gia đình (FamilyId) được chỉ định
    /// khi người dùng không phải là quản trị viên và có quyền truy cập vào gia đình đó.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService để IsAdmin trả về false. Thiết lập _mockUser với một User ID hợp lệ.
    ///               Thêm các Family và Event vào DB. Thiết lập _mockAuthorizationService để CanAccessFamily trả về true cho FamilyId cụ thể.
    ///               Tạo một GetUpcomingEventsQuery với FamilyId cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện có FamilyId khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng người dùng không phải quản trị viên
    /// chỉ có thể xem các sự kiện thuộc về gia đình mà họ có quyền truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByFamilyId_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var accessibleFamily = _fixture.Create<Family>();
        var inaccessibleFamily = _fixture.Create<Family>();
        _context.Families.AddRange(accessibleFamily, inaccessibleFamily);

        _context.FamilyUsers.Add(new FamilyUser { FamilyId = accessibleFamily.Id, UserProfileId = userId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.FamilyId, accessibleFamily.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(1))
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.FamilyId, inaccessibleFamily.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(2))
            .Create();
        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUpcomingEventsQuery
        {
            FamilyId = accessibleFamily.Id,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Id.Should().Be(event1.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các sự kiện theo phạm vi ngày (StartDate và EndDate) được chỉ định.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService để IsAdmin trả về false. Thiết lập _mockUser với một User ID hợp lệ.
    ///               Thêm các Family và Event vào DB. Thêm FamilyUser để cấp quyền truy cập cho User vào Family cụ thể.
    ///               Tạo một GetUpcomingEventsQuery với phạm vi ngày cụ thể.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện nằm trong phạm vi ngày.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo phạm vi ngày hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByDateRange()
    {
        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var family = _fixture.Create<Family>();
        _context.Families.Add(family);
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family.Id, UserProfileId = userId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.FamilyId, family.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(-5))
            .With(e => e.EndDate, DateTime.UtcNow.AddDays(-4))
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.FamilyId, family.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(1))
            .With(e => e.EndDate, DateTime.UtcNow.AddDays(2))
            .Create();
        var event3 = _fixture.Build<Event>()
            .With(e => e.FamilyId, family.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(5))
            .With(e => e.EndDate, DateTime.UtcNow.AddDays(6))
            .Create();

        _context.Events.AddRange(event1, event2, event3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUpcomingEventsQuery
        {
            FamilyId = family.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Id.Should().Be(event2.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler chỉ trả về các sự kiện từ các gia đình mà người dùng có quyền truy cập
    /// khi người dùng không phải là quản trị viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockAuthorizationService để IsAdmin trả về false. Thiết lập _mockUser với một User ID hợp lệ.
    ///               Thêm các Family và Event vào DB. Thêm FamilyUser để cấp quyền truy cập cho User vào một Family cụ thể.
    ///               Tạo một GetUpcomingEventsQuery với phạm vi ngày bao gồm tất cả các sự kiện.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách sự kiện chỉ chứa các sự kiện từ gia đình mà người dùng có quyền truy cập.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng các hạn chế về quyền truy cập của người dùng
    /// không phải quản trị viên được áp dụng chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessibleEvents_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthorizationService.Setup(s => s.IsAdmin()).Returns(false);
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var accessibleFamily = _fixture.Create<Family>();
        var inaccessibleFamily = _fixture.Create<Family>();
        _context.Families.AddRange(accessibleFamily, inaccessibleFamily);

        _context.FamilyUsers.Add(new FamilyUser { FamilyId = accessibleFamily.Id, UserProfileId = userId });
        await _context.SaveChangesAsync(CancellationToken.None);

        var event1 = _fixture.Build<Event>()
            .With(e => e.FamilyId, accessibleFamily.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(1))
            .Create();
        var event2 = _fixture.Build<Event>()
            .With(e => e.FamilyId, inaccessibleFamily.Id)
            .With(e => e.StartDate, DateTime.UtcNow.AddDays(2))
            .Create();
        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetUpcomingEventsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value!.First().Id.Should().Be(event1.Id);
    }
}
