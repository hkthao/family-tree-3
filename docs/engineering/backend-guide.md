# Hướng dẫn Phát triển Backend

Tài liệu này cung cấp hướng dẫn chi tiết về cách thiết lập môi trường, phát triển, kiểm thử và duy trì phần backend của ứng dụng Dòng Họ Việt.

## 1. Tổng quan Backend

Backend của ứng dụng Dòng Họ Việt đóng vai trò cung cấp các API cho frontend, xử lý logic nghiệp vụ phức tạp, quản lý dữ liệu, xác thực và ủy quyền người dùng.

**Công nghệ chính:**
*   **Framework:** ASP.NET 8
*   **Kiến trúc:** Clean Architecture
*   **Ngôn ngữ:** C#
*   **Mẫu thiết kế:** CQRS (Command Query Responsibility Segregation) với MediatR
*   **ORM:** Entity Framework Core
*   **Cơ sở dữ liệu:** MySQL
*   **Xác thực:** JWT Authentication
*   **Các dịch vụ AI/ML:** Tích hợp với các microservices xử lý hình ảnh (face-service), giọng nói (voice-service), OCR (ocr-service) và thông báo (notification-service).

### Cấu trúc thư mục `apps/backend/src/` (Clean Architecture)

Dự án backend tuân thủ Clean Architecture với các lớp chính:

*   **`Domain`**: Chứa các thực thể cốt lõi, giá trị đối tượng, enum, giao diện và các quy tắc nghiệp vụ.
*   **`Application`**: Chứa logic nghiệp vụ ứng dụng, Commands, Queries, Handlers, DTOs và các dịch vụ ứng dụng.
*   **`Infrastructure`**: Chứa các chi tiết triển khai như truy cập dữ liệu (EF Core), dịch vụ bên ngoài.
*   **`Web`**: Điểm vào của ứng dụng, chứa API Controllers, cấu hình xác thực và HTTP.
*   **`CompositionRoot`**: Quản lý việc đăng ký Dependency Injection.

## 2. Cài đặt và Chạy Backend

### 2.1. Yêu cầu môi trường

*   **.NET 8 SDK**: Phiên bản 8.0.x (hoặc mới hơn).
*   **Docker & Docker Compose**: (Tùy chọn) để chạy MySQL hoặc toàn bộ hệ thống.
*   **Công cụ CLI**: `dotnet-ef` để quản lý Entity Framework Core migrations: `dotnet tool install --global dotnet-ef`.

### 2.2. Chạy Backend cục bộ (riêng lẻ)

1.  **Điều hướng đến thư mục backend:**
    ```bash
    cd apps/backend
    ```
2.  **Cập nhật các gói NuGet:**
    ```bash
    dotnet restore
    ```
3.  **Khởi động MySQL:** Đảm bảo có một instance MySQL đang chạy. Bạn có thể sử dụng Docker Compose để khởi động dịch vụ MySQL:
    ```bash
    docker-compose -f ../../infra/docker-compose.yml up -d mysql
    ```
4.  **Cấu hình chuỗi kết nối:** Mở `apps/backend/src/Web/appsettings.json` và đảm bảo `DefaultConnection` trỏ đến MySQL. Ví dụ: `Server=localhost;Port=3306;Database=familytree;Uid=root;Pwd=password;` (hoặc `Server=mysql;` nếu chạy với Docker Compose).
5.  **Áp dụng Migrations:**
    ```bash
    dotnet ef database update --project src/Infrastructure --startup-project src/Web
    ```
6.  **Chạy ứng dụng:**
    ```bash
    dotnet run --project src/Web
    ```
    API sẽ khả dụng tại `http://localhost:8080` và Swagger UI tại `http://localhost:8080/swagger`.

### 2.3. Chạy toàn bộ hệ thống với Docker Compose

Đây là cách khuyến nghị để chạy tất cả các dịch vụ (backend, frontend, database, microservices) trong môi trường phát triển. Tham khảo `README.md` chính của dự án để biết hướng dẫn chi tiết.

```bash
cd ../.. # Quay về thư mục gốc của dự án
docker-compose -f infra/docker-compose.yml up --build
```

## 3. Quy trình Phát triển Tính năng Backend

1.  **Tạo nhánh mới:** Luôn làm việc trên một nhánh tính năng mới (ví dụ: `feature/ten-tinh-nang-moi-backend`).
2.  **Phát triển:**
    *   Thêm hoặc sửa đổi các thực thể trong `Domain`.
    *   Triển khai logic nghiệp vụ trong `Application` (Commands, Queries, Handlers).
    *   Cập nhật hoặc thêm các triển khai cơ sở dữ liệu/dịch vụ trong `Infrastructure`.
    *   Tạo hoặc sửa đổi các API Controller trong `Web` để lộ ra các chức năng mới.
3.  **Đảm bảo tuân thủ Code Style:**
    ```bash
    dotnet format
    ```
4.  **Viết và chạy Unit Tests:** Xem phần 4. Kiểm thử Backend.
5.  **Tạo Migration mới (nếu cần):** Xem phần 5. Quản lý Database.
6.  **Tạo Pull Request:** Khi hoàn thành, tạo một Pull Request lên nhánh `develop`.

## 4. Kiểm thử Backend

Dự án backend bao gồm Unit Tests và Integration Tests.

### 4.1. Cấu trúc Test

*   **`tests/Domain.UnitTests`**: Unit tests cho lớp `Domain`.
*   **`tests/Application.UnitTests`**: Unit tests cho lớp `Application`.
*   **`tests/Web.IntegrationTests`**: (Nếu có) Integration tests cho API Controllers.

### 4.2. Chạy Tests

```bash
cd apps/backend
dotnet test
```

Để chạy tests với độ bao phủ mã:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 5. Quản lý Database (Entity Framework Core Migrations)

Các lệnh này cần được chạy từ thư mục `apps/backend/` hoặc thư mục gốc của dự án (với các tùy chọn `--project` và `--startup-project` phù hợp).

### 5.1. Tạo migration mới

```bash
dotnet ef migrations add [MigrationName] --project src/Infrastructure --startup-project src/Web
```

### 5.2. Áp dụng migration

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

### 5.3. Seeding Dữ liệu

Dự án được cấu hình để tự động seed dữ liệu mẫu khi Backend khởi động ở chế độ `Development`, nếu database chưa có dữ liệu. Cơ chế này được triển khai trong `ApplicationDbContextInitialiser`.

## 6. Sử dụng Dịch vụ Phân quyền (Authorization Service)

Hệ thống cung cấp một dịch vụ phân quyền tập trung (`IAuthorizationService`) để kiểm tra quyền hạn của người dùng.

### 6.1. Inject `IAuthorizationService`

Bạn có thể inject `IAuthorizationService` vào các service, handler hoặc controller.

```csharp
public class FamilyService : IFamilyService
{
    private readonly IAuthorizationService _authorizationService;
    // ...
    public FamilyService(IAuthorizationService authorizationService, ...)
    {
        _authorizationService = authorizationService;
        // ...
    }

    public async Task<Result<bool>> UpdateFamily(Guid familyId, UpdateFamilyCommand command, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null) return Result<bool>.Failure("User not found.", 404);

        if (!_authorizationService.CanManageFamily(familyId, currentUserProfile))
        {
            return Result<bool>.Failure("User does not have permission to manage this family.", 403);
        }
        // ...
        return Result<bool>.Success(true);
    }
}
```

### 6.2. Các phương thức kiểm tra quyền tiêu biểu

*   `IsAdmin()`: Kiểm tra vai trò `Administrator` toàn cục.
*   `GetCurrentUserProfileAsync()`: Lấy thông tin `UserProfile` của người dùng hiện tại.
*   `CanAccessFamily(Guid familyId, UserProfile userProfile)`: Kiểm tra quyền truy cập vào một gia đình.
*   `CanManageFamily(Guid familyId, UserProfile userProfile)`: Kiểm tra quyền quản lý một gia đình.

## 7. Sử dụng AutoMapper cho DTO Mapping

Dự án sử dụng AutoMapper để tự động ánh xạ dữ liệu giữa các đối tượng (Entity <-> DTO).

### 7.1. Cấu hình Mapping

Định nghĩa trong các lớp kế thừa từ `Profile` của AutoMapper (ví dụ: `MappingProfile.cs` trong `Application/Common/Mappings`).

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Member, MemberDto>();
        // ...
    }
}
```

### 7.2. Inject và Sử dụng `IMapper`

```csharp
public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, Result<FamilyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FamilyDto>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var family = await _context.Families.FindAsync(request.Id);
        if (family == null) return Result<FamilyDto>.Failure("Family not found.", 404);
        return Result<FamilyDto>.Success(_mapper.Map<FamilyDto>(family));
    }
    }
```

## 8. CI/CD Backend

Backend được tích hợp vào quy trình CI/CD của GitHub Actions.

*   **Workflow CI (`.github/workflows/ci.yml`)**: Thực hiện build, test và kiểm tra định dạng mã nguồn.
*   **Workflows CD (`.github/workflows/cd-backend.yml`)**: Xây dựng Docker image cho backend và đẩy lên Docker Hub.

## 9. Xử lý sự cố (Troubleshooting)

*   **Lỗi kết nối cơ sở dữ liệu:** Kiểm tra chuỗi kết nối, trạng thái MySQL, và tường lửa.
*   **Lỗi Migrations:** Đảm bảo `dotnet ef` được cài đặt, kiểm tra Entity thay đổi.
*   **Lỗi phụ thuộc (Dependency Injection):** Kiểm tra `DependencyInjection.cs` trong các lớp.
*   **Lỗi 401/403 (Unauthorized/Forbidden):** Kiểm tra token JWT và quyền hạn người dùng.