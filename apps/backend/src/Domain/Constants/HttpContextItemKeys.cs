namespace backend.Domain.Constants;

/// <summary>
/// Chứa các khóa hằng số được sử dụng để lưu trữ và truy xuất dữ liệu từ HttpContext.Items.
/// </summary>
public static class HttpContextItemKeys
{
    /// <summary>
    /// Khóa cho UserId của người dùng hiện tại.
    /// </summary>
    public const string UserId = "UserId";

    /// <summary>
    /// Khóa cho ProfileId đang hoạt động của người dùng hiện tại.
    /// </summary>
    public const string ProfileId = "ProfileId";

    /// <summary>
    /// Khóa để đánh dấu rằng thông tin người dùng đã được xử lý bởi middleware.
    /// </summary>
    public const string UserInfoProcessed = "UserInfoProcessed";
    public const string NovuSubscriberProcessed = "NovuSubscriberProcessed";
}
