namespace backend.Application.Common.Interfaces;

/// <summary>
/// Giao diện cho một factory tạo ra các thể hiện của INotificationProvider.
/// </summary>
public interface INotificationProviderFactory
{
    /// <summary>
    /// Lấy một thể hiện của INotificationProvider dựa trên tên nhà cung cấp.
    /// </summary>
    /// <param name="providerName">Tên của nhà cung cấp thông báo (ví dụ: "Novu", "Firebase").</param>
    /// <returns>Một thể hiện của INotificationProvider.</returns>
    INotificationProvider GetProvider(string providerName);
}
