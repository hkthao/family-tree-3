namespace backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;

public class GetNotificationTemplatesQueryValidator : AbstractValidator<GetNotificationTemplatesQuery>
{
    public GetNotificationTemplatesQueryValidator()
    {
        RuleFor(v => v.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(v => v.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Kích thước trang phải lớn hơn hoặc bằng 1.");

        RuleFor(v => v.SearchQuery)
            .MaximumLength(200).WithMessage("Chuỗi tìm kiếm không được vượt quá 200 ký tự.");

        RuleFor(v => v.SortBy)
            .MaximumLength(50).WithMessage("Trường sắp xếp không được vượt quá 50 ký tự.");

        RuleFor(v => v.SortOrder)
            .Must(BeValidSortOrder).WithMessage("Thứ tự sắp xếp không hợp lệ. Chỉ chấp nhận 'asc' hoặc 'desc'.");

        RuleFor(v => v.LanguageCode)
            .MaximumLength(10).WithMessage("Mã ngôn ngữ không được vượt quá 10 ký tự.");
    }

    private bool BeValidSortOrder(string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            return true; // Null or empty is allowed (no sorting)
        }
        return sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
               sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);
    }
}
