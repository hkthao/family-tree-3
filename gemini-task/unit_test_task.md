Dưới đây là **phiên bản hoàn chỉnh, format chuẩn Markdown (.md)** của prompt cho **Gemini CLI** — giúp tự động viết lại toàn bộ Unit Test và Integration Test phù hợp với refactor mới của dự án .NET DDD + CQRS:

---

# 🧠 Prompt Gemini CLI — Sinh Unit Test & Integration Test cho Dự án ASP.NET Core (DDD + CQRS)

## 🎯 **Bối cảnh**

Dự án sử dụng **Entity Framework Core** với **DbContext trực tiếp** (❌ KHÔNG dùng Repository Pattern).

Framework test:

* **xUnit** + **FluentAssertions**
* Có thể dùng **AutoMoq** để mock dependency phụ (KHÔNG mock DbContext)
* **EF InMemoryDatabase** (`UseInMemoryDatabase(Guid.NewGuid().ToString())`) để mô phỏng database thật
* Mỗi test **chạy độc lập**, không dùng chung dữ liệu

---

## 🧩 **Phạm vi test**

Viết test cho **từng CommandHandler, QueryHandler, hoặc Service** trong thư mục:

```
Application.UnitTests/<Module>/<Feature>/<FeatureName>Tests.cs
```

Mỗi file test chỉ tập trung vào **Cac case tiêu biểu**:

* ❌ Entity không tồn tại → throw `NotFoundException`
* ✅ Dữ liệu hợp lệ → trả kết quả hoặc cập nhật chính xác
* 🚫 Quyền hoặc dữ liệu không hợp lệ → trả lỗi phù hợp

---

## 🧱 **Cấu trúc test**

### 🔹 Add summary comment block đầu file

```csharp
/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của UpdateEventCommandHandler.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo handler phản hồi đúng khi dữ liệu hợp lệ hoặc khi entity không tồn tại.
/// </summary>
```

### 🔹 Mỗi test method cần có comment chi tiết:

```csharp
// 🎯 Mục tiêu: Kiểm tra handler ném lỗi khi không tìm thấy Event.
// ⚙️ Arrange: Tạo context rỗng, khởi tạo handler.
// ⚙️ Act: Gọi Handle với Id không tồn tại.
// ⚙️ Assert: Kỳ vọng NotFoundException.
// 💡 Giải thích: Vì entity không tồn tại nên handler phải ném lỗi NotFound.
```

### 🔹 Đặt tên test rõ ràng:

* `Handle_ShouldThrowNotFoundException_WhenEventNotFound`
* `Handle_ShouldUpdateEventSuccessfully_WhenValidRequest`
* `Handle_ShouldReturnForbidden_WhenUserNotAuthorized`

---

## 🧩 **Cách setup dữ liệu**

Không mock `DbSet` hoặc các method như `FirstOrDefaultAsync`.

Tạo dữ liệu test bằng:

* Seed entity thủ công, **gán Id/FK hợp lệ**
* Hoặc dùng **AutoFixture** (nhưng phải gán FK thủ công nếu có quan hệ)

Mỗi test dùng **database mới**:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
```

---

## 🧰 **BaseTest Class**

Tạo `BaseTest` dùng chung cho tất cả:

```csharp
public abstract class BaseTest
{
    protected readonly AppDbContext _context;
    protected readonly IMapper _mapper;
    protected readonly Mock<IAuthorizationService> _authMock;

    protected BaseTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _authMock = new Mock<IAuthorizationService>();
    }
}
```

Các test kế thừa `BaseTest` để giảm lặp code.

---

## 🧩 **Phần mở rộng bổ sung**

Bổ sung các **loại test khác** phù hợp refactor mới:

### 1️⃣ **Authorization & Validation**

* Khi user chưa login → trả lỗi `Unauthorized`
* Khi user không có quyền → `Forbidden`
* Khi request thiếu field bắt buộc → `ValidationException`

### 2️⃣ **Integration Test**

* Mô phỏng pipeline thực qua **Mediator.Send(...)**
* Test transaction logic thật, không mock handler
* Dùng database thật (InMemory) và real DI container

### 3️⃣ **Domain Event Test**

* Kiểm tra khi entity thay đổi, **DomainEvent** được publish
* Assert rằng event xuất hiện trong `entity.DomainEvents`

### 4️⃣ **Audit Field Auto-set**

* Khi SaveChanges, kiểm tra `CreatedBy`, `UpdatedBy` tự động set đúng
* Đảm bảo dữ liệu được cập nhật đồng nhất giữa các handler

### 5️⃣ **Performance / Consistency**

* Dữ liệu sau Update không bị trùng hoặc ghi đè ngoài ý muốn
* EventMembers hoặc ChildEntities được cập nhật đúng số lượng

---

## ⚙️ **Nguyên tắc thực thi**

1. Viết từng test nhỏ, chạy pass trước khi sang handler khác.
2. Không thêm field giả (CreatedAt, UpdatedAt, IsDeleted...) nếu không có trong model thật.
3. Mỗi test có comment rõ ràng (Arrange / Act / Assert / Explain).
4. Dễ hiểu với **junior developer** hoặc **tester không chuyên backend**.
5. Giữ style đồng nhất với các test đã pass trước đó.

---

## 📁 **Kết quả mong muốn**

* Mỗi test chạy độc lập, pass ổn định.
* Có giải thích dễ hiểu.
* Sử dụng đúng FluentAssertions (`result.Should().BeTrue();`).
* Tất cả test chạy async (`await handler.Handle(...);`).
* Cấu trúc rõ ràng, dễ maintain, dễ mở rộng thêm test mới.

---

## ⚙️ **Mục tiêu cuối cùng**

> Giúp tôi — một developer làm việc một mình — có thể:
>
> * Viết test nhanh, đúng, dễ hiểu
> * Đảm bảo handler refactor vẫn hoạt động chính xác
> * Không cần đoán mô hình hoặc thêm property giả
> * Tạo ra test tự động hóa hữu ích cho CI/CD pipeline