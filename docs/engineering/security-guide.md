# Hướng dẫn Bảo mật và Kiểm soát Truy cập

Tài liệu này mô tả các biện pháp bảo mật và cơ chế kiểm soát truy cập được áp dụng trong dự án.

## 1. Xác thực (Authentication)

-   **Cơ chế**: Sử dụng JSON Web Tokens (JWT) để xác thực người dùng.
-   **Luồng hoạt động**:
    1.  Người dùng đăng nhập bằng email và mật khẩu.
    2.  Backend xác thực thông tin, nếu thành công, tạo ra một JWT token.
    3.  Token được trả về cho Frontend và lưu trữ an toàn (ví dụ: trong `localStorage` hoặc `cookie`).
    4.  Với mỗi yêu cầu cần xác thực, Frontend gửi token trong header `Authorization`.

## 2. Phân quyền (Authorization)

-   **Cơ chế**: Sử dụng Role-Based Access Control (RBAC).
-   **Các vai trò (Roles)**:
    -   `Admin`: Có toàn quyền quản lý hệ thống.
    -   `FamilyAdmin`: Có quyền quản lý một hoặc nhiều gia đình.
    -   `Member`: Thành viên thông thường, chỉ có quyền xem.

-   **Triển khai**: Sử dụng `[Authorize]` attribute trong ASP.NET Core để bảo vệ các endpoint.

    ```csharp
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliesController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin,FamilyAdmin")]
        public async Task<IActionResult> GetFamilies()
        {
            // ...
        }
    }
    ```

## 3. Các biện pháp bảo mật khác

-   **HTTPS**: Toàn bộ giao tiếp giữa Frontend và Backend đều được mã hóa bằng HTTPS.
-   **CORS (Cross-Origin Resource Sharing)**: Cấu hình CORS chặt chẽ để chỉ cho phép các domain được tin tưởng truy cập API.
-   **Input Validation**: Tất cả dữ liệu từ người dùng đều được validate cẩn thận ở cả Frontend và Backend để chống lại các cuộc tấn công như XSS, SQL Injection.
-   **Password Hashing**: Mật khẩu người dùng được hash bằng thuật toán an toàn (ví dụ: bcrypt) trước khi lưu vào database.
