### 🎯 Mục tiêu
Tạo class `SpecificationBuilder<T>` cho dự án backend theo pattern Specification hiện có trong thư mục `Application/Common/Specifications`.

Mục tiêu của builder:
- Cho phép xây dựng specification một cách **fluent** và **type-safe**.
- Kết hợp các phần **filter (Criteria)**, **include**, **order by**, **paging**.
- Có thể build ra một instance của `BaseSpecification<T>` tương thích với repository hiện tại.
- Không thay đổi hoặc phá cấu trúc `BaseSpecification<T>`, `ISpecification<T>`, hay các spec cũ.
- Code phải **dễ hiểu cho junior**, có **comment chi tiết**, và **viết theo C# 12**.

---

### ⚙️ Yêu cầu kỹ thuật

1. **Vị trí file:**
   - `Application/Common/Specifications/SpecificationBuilder.cs`

2. **Phạm vi:**
   - Dùng `System.Linq.Expressions` để giữ type safety.
   - Hỗ trợ chuỗi gọi fluent:
     ```csharp
     var spec = new SpecificationBuilder<User>()
         .Filter(u => u.IsActive)
         .Include(u => u.Families)
         .OrderByDescending(u => u.CreatedAt)
         .Page(1, 20)
         .Build();
     ```

3. **Phương thức builder cần có:**
   | Method | Chức năng |
   |---------|-----------|
   | `Filter(Expression<Func<T, bool>> expression)` | Thêm tiêu chí lọc. |
   | `Include(Expression<Func<T, object>> include)` | Thêm include theo navigation property. |
   | `OrderBy(Expression<Func<T, object>> keySelector)` | Thêm sắp xếp tăng dần. |
   | `OrderByDescending(Expression<Func<T, object>> keySelector)` | Thêm sắp xếp giảm dần. |
   | `Page(int pageIndex, int pageSize)` | Áp dụng phân trang. |
   | `Build()` | Tạo ra instance `BaseSpecification<T>` cuối cùng. |

4. **Nguyên tắc khi build:**
   - Nếu có nhiều `Filter` → nối bằng `AND`.
   - `OrderBy` và `OrderByDescending` có thể gọi nhiều lần (ưu tiên theo thứ tự gọi).
   - Nếu không có `Page` thì `IsPagingEnabled = false`.
   - Nếu gọi `Build()` nhiều lần → luôn tạo object mới, không mutate instance cũ.

5. **Yêu cầu thêm:**
   - Có region hoặc XML doc comment để junior dễ đọc.
   - Viết Unit Test mẫu cho class này (nếu có thể) ở `Application.UnitTests/Specifications/SpecificationBuilderTests.cs`.

---

### 💡 Lưu ý quan trọng
- Không được thay đổi file `BaseSpecification<T>` hay `ISpecification<T>`.
- Nếu cần helper nội bộ → tạo class `SpecificationBuilderExtensions` cùng thư mục, không chạm vào các spec domain hiện có.
- Mọi logic phải **thread-safe** và **không giữ trạng thái ngoài scope builder**.
- Ưu tiên clarity hơn cleverness.

---

### ✅ Output mong đợi
- File `SpecificationBuilder.cs` đầy đủ, sẵn sàng build và chạy test.
- Có thể sử dụng như sau:

```csharp
var spec = new SpecificationBuilder<Family>()
    .Filter(f => f.Visibility == "public")
    .OrderBy(f => f.Name)
    .Page(1, 10)
    .Build();
