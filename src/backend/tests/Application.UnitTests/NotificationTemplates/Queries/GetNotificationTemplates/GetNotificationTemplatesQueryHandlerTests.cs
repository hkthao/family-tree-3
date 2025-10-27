using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplates;

public class GetNotificationTemplatesQueryHandlerTests : TestBase
{
    private readonly GetNotificationTemplatesQueryHandler _handler;

    public GetNotificationTemplatesQueryHandlerTests()
    {
        _handler = new GetNotificationTemplatesQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về tất cả các mẫu thông báo
    /// khi không có bộ lọc nào được áp dụng.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo vào DB. Tạo một GetNotificationTemplatesQuery rỗng.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chứa tất cả các mẫu đã thêm.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng hệ thống có thể truy xuất
    /// tất cả các mẫu thông báo khi không có bộ lọc nào được áp dụng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllNotificationTemplates_WhenNoFilters()
    {
        // Arrange
        var templates = _fixture.CreateMany<NotificationTemplate>(5).ToList();
        _context.NotificationTemplates.AddRange(templates);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(5);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo chuỗi tìm kiếm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo vào DB, một số khớp với chuỗi tìm kiếm.
    ///               Tạo một GetNotificationTemplatesQuery với chuỗi tìm kiếm.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chức năng tìm kiếm hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterBySearchQuery()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Subject A").With(nt => nt.Body, "Body Content").Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Subject B").With(nt => nt.Body, "Another Body").Create();
        var template3 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Other Subject").With(nt => nt.Body, "Body A").Create();
        _context.NotificationTemplates.AddRange(template1, template2, template3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { SearchQuery = "Body A" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Subject.Should().Be(template3.Subject);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo loại sự kiện (EventType).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các loại sự kiện khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với EventType cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo loại sự kiện hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByEventType()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.EventType, NotificationType.FamilyCreated).Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.EventType, NotificationType.MemberCreated).Create();
        _context.NotificationTemplates.AddRange(template1, template2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { EventType = NotificationType.FamilyCreated };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().EventType.Should().Be(NotificationType.FamilyCreated);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo kênh (Channel).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các kênh khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với Channel cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo kênh hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByChannel()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.Channel, NotificationChannel.Email).Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.Channel, NotificationChannel.SMS).Create();
        _context.NotificationTemplates.AddRange(template1, template2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { Channel = NotificationChannel.Email };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Channel.Should().Be(NotificationChannel.Email);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo định dạng (Format).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các định dạng khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với Format cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo định dạng hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByFormat()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.Format, TemplateFormat.Html).Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.Format, TemplateFormat.PlainText).Create();
        _context.NotificationTemplates.AddRange(template1, template2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { Format = TemplateFormat.Html };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().Format.Should().Be(TemplateFormat.Html);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo mã ngôn ngữ (LanguageCode).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các mã ngôn ngữ khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với LanguageCode cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo mã ngôn ngữ hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByLanguageCode()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.LanguageCode, "en").Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.LanguageCode, "vi").Create();
        _context.NotificationTemplates.AddRange(template1, template2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { LanguageCode = "en" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().LanguageCode.Should().Be("en");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lọc các mẫu thông báo theo trạng thái hoạt động (IsActive).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các trạng thái hoạt động khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với IsActive cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chỉ chứa các mẫu khớp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng bộ lọc theo trạng thái hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterByIsActive()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.IsActive, true).Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.IsActive, false).Create();
        _context.NotificationTemplates.AddRange(template1, template2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { IsActive = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.First().IsActive.Should().BeTrue();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler áp dụng sắp xếp (SortBy và SortOrder).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm một số mẫu thông báo với các giá trị Subject khác nhau vào DB.
    ///               Tạo một GetNotificationTemplatesQuery với SortBy và SortOrder cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo được sắp xếp đúng.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chức năng sắp xếp hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldApplySorting()
    {
        // Arrange
        var template1 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Subject C").Create();
        var template2 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Subject A").Create();
        var template3 = _fixture.Build<NotificationTemplate>().With(nt => nt.Subject, "Subject B").Create();
        _context.NotificationTemplates.AddRange(template1, template2, template3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { SortBy = "Subject", SortOrder = "asc" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items[0].Subject.Should().Be("Subject A");
        result.Value.Items[1].Subject.Should().Be("Subject B");
        result.Value.Items[2].Subject.Should().Be("Subject C");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler áp dụng phân trang (PageNumber và PageSize).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thêm nhiều mẫu thông báo vào DB. Tạo một GetNotificationTemplatesQuery với PageNumber và PageSize cụ thể.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công và danh sách mẫu thông báo chứa các mẫu đúng cho trang được yêu cầu.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng chức năng phân trang hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // Arrange
        var templates = _fixture.CreateMany<NotificationTemplate>(10).OrderBy(nt => nt.Created).ToList();
        _context.NotificationTemplates.AddRange(templates);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetNotificationTemplatesQuery { PageNumber = 2, PageSize = 3, SortBy = "Created", SortOrder = "asc" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Items.First().Id.Should().Be(templates[3].Id);
        result.Value.Items.Last().Id.Should().Be(templates[5].Id);
    }
}
