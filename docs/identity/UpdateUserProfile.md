# Cập nhật Hồ sơ Người dùng (User Profile Update)

Tài liệu này mô tả cách hệ thống xử lý việc cập nhật thông tin hồ sơ người dùng, với kiến trúc linh hoạt cho phép tích hợp nhiều nhà cung cấp xác thực (Auth Provider).

## 1. Tổng quan

Tính năng cập nhật hồ sơ người dùng cho phép người dùng thay đổi các thông tin cá nhân như tên, ảnh đại diện (avatar URL), và email (tùy thuộc vào cấu hình và quyền hạn). Kiến trúc được thiết kế để dễ dàng mở rộng hoặc thay thế nhà cung cấp xác thực mà không ảnh hưởng đến logic nghiệp vụ cốt lõi.

## 2. Kiến trúc

Hệ thống sử dụng **Strategy Pattern** để chọn nhà cung cấp xác thực phù hợp dựa trên cấu hình. `IAuthProvider` là interface chung định nghĩa các hoạt động xác thực và quản lý người dùng, trong khi các lớp cụ thể như `Auth0Provider` triển khai interface này cho từng nhà cung cấp.

### 2.1. Interface `IAuthProvider`

Interface này được định nghĩa trong `backend/src/Application/Common/Interfaces/IAuthProvider.cs` và bao gồm phương thức `UpdateUserProfileAsync`:

```csharp
// backend/src/Application/Common/Interfaces/IAuthProvider.cs
public interface IAuthProvider
{
    // ... các phương thức khác ...
    Task<Result> UpdateUserProfileAsync(string userId, UserProfileUpdateRequest request);
}
```

-   `userId`: ID của người dùng cần cập nhật (thường là `Auth0UserId` hoặc tương đương).
-   `request`: Đối tượng `UserProfileUpdateRequest` chứa các thông tin cần cập nhật.
-   `Result`: Đối tượng `Result` chuẩn của hệ thống, cho biết thao tác thành công hay thất bại.

### 2.2. `UserProfileUpdateRequest` DTO

Đối tượng này định nghĩa các trường thông tin có thể được cập nhật, nằm trong `backend/src/Application/Identity/DTOs/UserProfileUpdateRequest.cs`:

```csharp
// backend/src/Application/Identity/DTOs/UserProfileUpdateRequest.cs
public class UserProfileUpdateRequest
{
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, object>? UserMetadata { get; set; }
}
```

### 2.3. Triển khai `Auth0Provider`

`Auth0Provider` (trong `backend/src/Infrastructure/Auth/Auth0Provider.cs`) triển khai `IAuthProvider` và tương tác với **Auth0 Management API** để cập nhật hồ sơ người dùng. Hiện tại, đây là một mock implementation để phục vụ mục đích phát triển và kiểm thử.

#### Các trường được hỗ trợ cập nhật (qua Auth0 Management API):

-   `name`: Tên hiển thị của người dùng.
-   `picture`: URL ảnh đại diện.
-   `email`: Địa chỉ email (cần kiểm tra quyền và trạng thái xác minh).
-   `user_metadata`: Dữ liệu tùy chỉnh của người dùng.

#### Xử lý lỗi và Bảo mật (trong triển khai thực tế):

-   **Rate Limit (HTTP 429)**: Auth0 Management API có giới hạn tần suất gọi. Triển khai thực tế cần có cơ chế retry với exponential backoff.
-   **Token Expiration**: Đảm bảo access token cho Management API luôn hợp lệ.
-   **Unauthorized (401)**: Xử lý khi không có quyền truy cập Management API.

## 3. Luồng cập nhật Hồ sơ Người dùng

1.  **Frontend gửi yêu cầu**: Frontend gửi một yêu cầu cập nhật hồ sơ người dùng đến Backend API (ví dụ: `PUT /api/user-profiles/{userId}`).
2.  **Backend API nhận yêu cầu**: Controller nhận yêu cầu và gửi `UpdateUserProfileCommand` đến MediatR.
3.  **`UpdateUserProfileCommand`**: Chứa `userId` của người dùng cần cập nhật và `UserProfileUpdateRequest`.
4.  **`UpdateUserProfileCommandValidator`**: Sử dụng FluentValidation để xác thực dữ liệu đầu vào:
    -   `UserId` không được trống.
    -   `Name` không quá 250 ký tự.
    -   `Email` phải là định dạng email hợp lệ.
    -   `Picture` phải là URL hợp lệ.
5.  **`UpdateUserProfileCommandHandler`**: Xử lý logic cập nhật:
    -   **Kiểm tra quyền**: So sánh `userId` trong command với `_user.Id` (ID của người dùng hiện tại từ token) để đảm bảo người dùng chỉ cập nhật hồ sơ của chính họ.
    -   Gọi `_authProvider.UpdateUserProfileAsync(userId, request)` để thực hiện cập nhật thông qua nhà cung cấp xác thực đã cấu hình.
    -   Trả về `Result` thành công hoặc thất bại.

## 4. Cấu hình Nhà cung cấp Xác thực

Việc lựa chọn nhà cung cấp xác thực được cấu hình trong `appsettings.json`:

```json
{
  "AuthProvider": "Auth0"
}
```

-   `AuthProvider`: Tên của nhà cung cấp xác thực sẽ được sử dụng (ví dụ: `Auth0`).

Trong `backend/src/Infrastructure/DependencyInjection.cs`, một **Factory Pattern** được sử dụng để đăng ký triển khai `IAuthProvider` phù hợp dựa trên giá trị này.

```csharp
// backend/src/Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ... các đăng ký dịch vụ khác ...

        // Register IAuthProvider based on configuration
        var authProviderType = configuration["AuthProvider"];
        switch (authProviderType)
        {
            case "Auth0":
                services.AddSingleton<IAuthProvider, Auth0Provider>();
                break;
            // Add other providers here as needed
            default:
                throw new InvalidOperationException($"Auth provider '{authProviderType}' is not supported.");
        }

        // ... các đăng ký dịch vụ khác ...
        return services;
    }
}
```

## 5. Bảo mật và Kiểm soát Truy cập

-   **Chỉ cập nhật hồ sơ của chính mình**: `UpdateUserProfileCommandHandler` thực hiện kiểm tra nghiêm ngặt để đảm bảo `request.UserId` khớp với `_user.Id` (ID của người dùng đã xác thực).
-   **Giới hạn tần suất**: (Chưa triển khai trong mock, nhưng cần có trong triển khai thực tế) Có thể sử dụng middleware hoặc dịch vụ riêng để giới hạn số lần cập nhật trong một khoảng thời gian nhất định.
-   **Xác minh Email**: (Chưa triển khai trong mock) Trong triển khai thực tế, việc thay đổi email có thể yêu cầu xác minh lại email để đảm bảo bảo mật.
-   **Xác thực đầu vào**: `UpdateUserProfileCommandValidator` đảm bảo dữ liệu đầu vào hợp lệ trước khi xử lý.

## 6. Mở rộng với các Nhà cung cấp Xác thực khác

Để thêm một nhà cung cấp xác thực mới (ví dụ: Firebase, Azure AD):

1.  Tạo một lớp mới (ví dụ: `FirebaseProvider.cs`) triển khai interface `IAuthProvider`.
2.  Triển khai logic tương tác với API của nhà cung cấp đó trong lớp mới.
3.  Thêm một `case` mới vào `switch` statement trong `DependencyInjection.cs` để đăng ký `FirebaseProvider` khi `AuthProvider` trong cấu hình là `Firebase`.

## 7. Các lỗi Phổ biến và Mẫu Phản hồi

-   **401 Unauthorized**: Người dùng chưa được xác thực hoặc token không hợp lệ.
-   **403 Forbidden**: Người dùng đã xác thực nhưng không có quyền cập nhật hồ sơ của người khác.
-   **429 Too Many Requests**: Vượt quá giới hạn tần suất gọi API (từ nhà cung cấp xác thực).
-   **500 Internal Server Error**: Lỗi không mong muốn ở Backend hoặc từ nhà cung cấp xác thực.

Các phản hồi lỗi sẽ tuân theo [Cấu trúc Phản hồi Lỗi](#5-cấu-trúc-phản-hồi-lỗi-error-response) đã định nghĩa.
