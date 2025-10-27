using AutoFixture;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;
using FluentAssertions;
using Xunit;
using backend.Domain.Enums;

namespace backend.Application.UnitTests.NotificationTemplates.Queries.GetNotificationTemplates;

public class GetNotificationTemplatesQueryValidatorTests
{
    private readonly Fixture _fixture;

    public GetNotificationTemplatesQueryValidatorTests()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi PageNumber nhỏ hơn 1.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với PageNumber = 0.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Số trang phải lớn hơn hoặc bằng 1.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: PageNumber phải là một số dương.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_PageNumberLessThanOne_ShouldReturnValidationError()
    {
        // Arrange
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.PageNumber, 0)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageNumber" && e.ErrorMessage == "Số trang phải lớn hơn hoặc bằng 1.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi PageSize nhỏ hơn 1.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với PageSize = 0.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Kích thước trang phải lớn hơn hoặc bằng 1.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: PageSize phải là một số dương.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_PageSizeLessThanOne_ShouldReturnValidationError()
    {
        // Arrange
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.PageSize, 0)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize" && e.ErrorMessage == "Kích thước trang phải lớn hơn hoặc bằng 1.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi SearchQuery vượt quá độ dài tối đa (200 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với SearchQuery có độ dài lớn hơn 200 ký tự.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Chuỗi tìm kiếm không được vượt quá 200 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: SearchQuery có giới hạn độ dài tối đa.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_SearchQueryExceedsMaxLength_ShouldReturnValidationError()
    {
        // Arrange
        var longSearchQuery = new string('a', 201); // More than 200 characters
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.SearchQuery, longSearchQuery)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchQuery" && e.ErrorMessage == "Chuỗi tìm kiếm không được vượt quá 200 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi SortBy vượt quá độ dài tối đa (50 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với SortBy có độ dài lớn hơn 50 ký tự.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Trường sắp xếp không được vượt quá 50 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: SortBy có giới hạn độ dài tối đa.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_SortByExceedsMaxLength_ShouldReturnValidationError()
    {
        // Arrange
        var longSortBy = new string('a', 51); // More than 50 characters
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.SortBy, longSortBy)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SortBy" && e.ErrorMessage == "Trường sắp xếp không được vượt quá 50 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi SortOrder không hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với SortOrder không phải "asc" hoặc "desc".
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Thứ tự sắp xếp không hợp lệ. Chỉ chấp nhận 'asc' hoặc 'desc'.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: SortOrder chỉ chấp nhận "asc" hoặc "desc".
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_InvalidSortOrder_ShouldReturnValidationError()
    {
        // Arrange
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.SortOrder, "invalid")
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SortOrder" && e.ErrorMessage == "Thứ tự sắp xếp không hợp lệ. Chỉ chấp nhận 'asc' hoặc 'desc'.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator trả về lỗi khi LanguageCode vượt quá độ dài tối đa (10 ký tự).
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery với LanguageCode có độ dài lớn hơn 10 ký tự.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate có lỗi.
    ///              Kiểm tra xem thông báo lỗi chứa thông báo "Mã ngôn ngữ không được vượt quá 10 ký tự.".
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: LanguageCode có giới hạn độ dài tối đa.
    /// Validator phải phát hiện lỗi này và trả về thông báo lỗi phù hợp.
    /// </summary>
    [Fact]
    public async Task Validate_LanguageCodeExceedsMaxLength_ShouldReturnValidationError()
    {
        // Arrange
        var longLanguageCode = new string('a', 11); // More than 10 characters
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.LanguageCode, longLanguageCode)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LanguageCode" && e.ErrorMessage == "Mã ngôn ngữ không được vượt quá 10 ký tự.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng validator không trả về lỗi khi GetNotificationTemplatesQuery hợp lệ.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một GetNotificationTemplatesQuery hợp lệ.
    ///               Khởi tạo GetNotificationTemplatesQueryValidator.
    ///    - Act: Gọi phương thức Validate của validator với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả Validate không có lỗi.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Một query hợp lệ không nên gây ra lỗi validation.
    /// </summary>
    [Fact]
    public async Task Validate_ValidQuery_ShouldNotReturnValidationError()
    {
        // Arrange
        var query = _fixture.Build<GetNotificationTemplatesQuery>()
            .With(q => q.PageNumber, 1)
            .With(q => q.PageSize, 10)
            .With(q => q.SearchQuery, "valid search")
            .With(q => q.SortBy, "Subject")
            .With(q => q.SortOrder, "asc")
            .With(q => q.EventType, NotificationType.FamilyCreated)
            .With(q => q.Channel, NotificationChannel.Email)
            .With(q => q.Format, TemplateFormat.Html)
            .With(q => q.LanguageCode, "en")
            .With(q => q.IsActive, true)
            .Create();
        var validator = new GetNotificationTemplatesQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
