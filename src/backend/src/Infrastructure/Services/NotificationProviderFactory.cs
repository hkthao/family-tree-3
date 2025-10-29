using backend.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai INotificationProviderFactory để lấy các nhà cung cấp thông báo cụ thể.
/// </summary>
public class NotificationProviderFactory : INotificationProviderFactory
{
    private readonly IEnumerable<INotificationProvider> _providers;

    /// <summary>
    /// Khởi tạo một phiên bản mới của NotificationProviderFactory.
    /// </summary>
    /// <param name="providers">Tập hợp các nhà cung cấp thông báo đã đăng ký.</param>
    public NotificationProviderFactory(IEnumerable<INotificationProvider> providers)
    {
        _providers = providers;
    }

    /// <summary>
    /// Lấy một thể hiện của INotificationProvider dựa trên tên nhà cung cấp.
    /// </summary>
    /// <param name="providerName">Tên của nhà cung cấp thông báo (ví dụ: "Novu", "Firebase").</param>
    /// <returns>Một thể hiện của INotificationProvider.</returns>
    /// <exception cref="ArgumentException">Ném ra nếu không tìm thấy nhà cung cấp cho tên đã cho.</exception>
    public INotificationProvider GetProvider(string providerName)
    {
        // Hiện tại, chúng ta chỉ có một nhà cung cấp (Novu), vì vậy chúng ta sẽ trả về nó.
        // Trong tương lai, khi có nhiều nhà cung cấp, chúng ta sẽ cần một cách để phân biệt chúng.
        // Ví dụ: mỗi nhà cung cấp có thể có một thuộc tính 'Name' hoặc được bọc trong một lớp có tên.
        // For now, we'll just return the first (and only) provider.
        var provider = _providers.FirstOrDefault();
        if (provider == null)
        {
            throw new ArgumentException($"No notification provider found for '{providerName}'.");
        }
        return provider;
    }
}