# Hướng dẫn Backend

Tài liệu này cung cấp hướng dẫn chi tiết về cấu trúc, quy tắc và các phương pháp tốt nhất cho việc phát triển Backend của dự án.

## 1. Cấu trúc thư mục (Clean Architecture)

Dự án sử dụng kiến trúc Clean Architecture, chia thành 4 project chính:

```
backend/
├── src/
│   ├── Domain/         # Chứa các entity, value object, enum, và domain event
│   ├── Application/    # Chứa logic nghiệp vụ, DTOs, và các interface
│   ├── Infrastructure/ # Chứa các triển khai của interface (repository, services)
│   └── Web/            # Chứa API controllers và cấu hình ASP.NET Core
└── tests/              # Chứa các project test
```

## 2. Dependency Injection

-   Sử dụng built-in Dependency Injection của ASP.NET Core.
-   Các service và repository được đăng ký trong file `DependencyInjection.cs` của mỗi project.

## 3. Middleware

-   **Error Handling**: Middleware xử lý lỗi tập trung, bắt các exception và trả về response lỗi chuẩn.
-   **Authentication & Authorization**: Middleware xác thực JWT token và kiểm tra quyền truy cập.

## 4. Repository Pattern

-   Sử dụng Repository Pattern để tách biệt logic nghiệp vụ khỏi lớp truy cập dữ liệu.
-   Các interface của repository được định nghĩa trong `Application` layer.
-   Các triển khai cụ thể (sử dụng Entity Framework Core) được đặt trong `Infrastructure` layer.

### Ví dụ

**Interface trong Application Layer:**

```csharp
public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Family family, CancellationToken cancellationToken = default);
}
```

**Triển khai trong Infrastructure Layer:**

```csharp
public class FamilyRepository : IFamilyRepository
{
    private readonly ApplicationDbContext _context;

    public FamilyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ... triển khai các phương thức
}
```

## 5. Coding Style

-   Sử dụng `dotnet format` để đảm bảo code style nhất quán.
-   Tuân thủ các quy tắc đặt tên và coding convention của C#.
-   Viết comment XML cho các public API để Swagger có thể tự động tạo tài liệu.
