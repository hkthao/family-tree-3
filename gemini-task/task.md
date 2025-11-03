
Bạn là một kiến trúc sư phần mềm DDD, CQRS và ASP.NET Core chuyên nghiệp.
Hiện tại ứng dụng Family Tree có class `CurrentUser` đang trả về ProfileId thay vì UserId thật sự.
Vấn đề: Khi người dùng login lần đầu qua Auth0, `OnTokenValidated` sẽ lưu user vào database, nhưng đôi khi xảy ra deadlock vì quá trình này chạy trong cùng transaction auditing.

🎯 Mục tiêu refactor:
1. Tách biệt rõ **User** và **Profile** trong domain model.
2. Giữ nguyên cấu trúc domain hiện tại (Profile vẫn tồn tại, nhưng không làm gốc auditing).
3. Thêm middleware xử lý mapping `Auth0 sub → User → Profile` sau khi authentication.
4. Sử dụng **UserId** cho auditing (`CreatedBy`, `UpdatedBy`) thay vì ProfileId.
5. Cho phép lưu `ProfileId` vào `HttpContext.Items` để filter dữ liệu nếu cần.
6. Đảm bảo backward-compatible cho phần quyền (authorization) và dữ liệu cũ.

---

### ✅ Đầu vào hiện có

- `CurrentUser` đang dùng `IHttpContextAccessor` và `IApplicationDbContext`.
- `IUser` interface hiện có property `Id` (ProfileId).
- `OnTokenValidated` event hiện đang insert `UserProfile` vào DB.
- Audit interceptor (`DispatchDomainEventsInterceptor`) hiện dùng `CurrentUser.Id`.

---

### 🧩 Yêu cầu cụ thể khi refactor

1. **Tạo entity `User` mới**
   - Thuộc tính: `Id`, `AuthProviderId` (Auth0 sub), `Email`.
   - Navigation: `Profiles`.

2. **Cập nhật entity `Profile`**
   - Thêm `UserId` (foreign key tới `User`).
   - Mối quan hệ 1-nhiều giữa `User` và `Profile`.

3. **Cập nhật `IUser` → `ICurrentUser` interface**
   ```csharp
   public interface ICurrentUser
   {
       Guid UserId { get; }
       Guid? ProfileId { get; }
       string? Email { get; }
       string? Name { get; }
   }
````

4. **Viết lại `CurrentUser` class**

   * Không dùng `_context` để truy vấn DB trong getter.
   * Chỉ đọc claim và dữ liệu từ `HttpContext.Items`.

5. **Thêm middleware mới (sau authentication)**

   * Lấy claim `sub` từ Auth0 token.
   * Tìm hoặc tạo mới User trong DB.
   * Tìm Profile mặc định (nếu có).
   * Lưu `ActiveProfileId` vào `HttpContext.Items["ActiveProfileId"]`.

6. **Cập nhật Auditing**

   * Audit interceptor dùng `currentUser.UserId`.
   * Nếu không đăng nhập thì để `Guid.Empty`.

7. **Đảm bảo tương thích ngược**

   * Không xóa Profile cũ.
   * Mapping dữ liệu cũ: `UserProfile.ExternalId` → `User.AuthProviderId`.

---

### ⚙️ Kết quả mong muốn

* Không còn deadlock khi user login lần đầu.
* Auditing chuẩn (ghi `UserId` thay vì `ProfileId`).
* Authorization và data filter vẫn hoạt động nhờ `ActiveProfileId`.
* Middleware xử lý Auth0 mapping gọn, có log cảnh báo nếu thiếu thông tin.

---

Chú ý:

* Sử dụng async/await đúng cách, không block sync.
* Mọi truy cập DB trong middleware đều nằm ngoài transaction domain.
* Code phải clean, có comment và không phá kiến trúc CQRS hiện tại.

```

