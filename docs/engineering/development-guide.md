# Hướng dẫn Phát triển

## Mục lục

- [1. Yêu cầu môi trường](#1-yêu-cầu-môi-trường)
- [2. Cài đặt và Chạy dự án](#2-cài-đặt-và-chạy-dự-án)
  - [2.1. Clone repository](#21-clone-repository)
  - [2.2. Chạy với Docker Compose (Khuyến nghị)](#22-chạy-với-docker-compose-khuyến-nghị)
  - [2.3. Chạy riêng lẻ](#23-chạy-riêng-lẻ)
- [3. Build và Deploy](#3-build-và-deploy)
  - [3.1. Build Docker images](#31-build-docker-images)
  - [3.2. Deploy](#32-deploy)
- [4. Quản lý Database](#4-quản-lý-database)
  - [4.1. Tạo migration mới](#41-tạo-migration-mới)
  - [4.2. Áp dụng migration](#42-áp-dụng-migration)
- [5. Seeding a Database](#5-seeding-a-database)
- [6. Sử dụng Dịch vụ Phân quyền (Authorization Service)](#6-sử-dụng-dịch-vụ-phân-quyền-authorization-service)
  - [6.1. Inject `IAuthorizationService`](#61-inject-iauthorizationservice)
  - [6.2. Các phương thức kiểm tra quyền](#62-các-phương-thức-kiểm-tra-quyền)
  - [6.3. Lưu ý khi phát triển](#63-lưu-ý-khi-phát-triển)
- [7. Sử dụng AutoMapper cho DTO Mapping](#7-sử-dụng-automapper-cho-dto-mapping)

---

Tài liệu này hướng dẫn cách cài đặt môi trường, build, và deploy dự án Dòng Họ Việt.

## 1. Yêu cầu môi trường

Để phát triển và chạy dự án, bạn cần cài đặt các công cụ sau:

-   **Docker và Docker Compose**: Phiên bản mới nhất để chạy ứng dụng trong môi trường container.
-   **.NET 8 SDK**: Phiên bản 8.0.x (hoặc mới hơn) cho việc phát triển Backend.
-   **Node.js 20+**: Phiên bản 20.x (hoặc mới hơn) cho việc phát triển Frontend.
-   **Công cụ CLI**: `dotnet-ef` để quản lý Entity Framework Core migrations cho database nghiệp vụ. Bạn có thể cài đặt bằng lệnh: `dotnet tool install --global dotnet-ef`.

## 2. Cài đặt và Chạy dự án

### 2.1. Clone repository

```bash
git clone https://github.com/your-username/family-tree-3.git
cd family-tree-3
```

### 2.2. Chạy với Docker Compose (Khuyến nghị)

Đây là cách nhanh nhất và được khuyến nghị để chạy cả Frontend, Backend, và Database trong môi trường phát triển. Docker Compose sẽ xây dựng (nếu cần) và khởi động tất cả các services được định nghĩa trong `infra/docker-compose.yml`.

```bash
docker-compose -f infra/docker-compose.yml up --build
```

Sau khi chạy lệnh trên và các services đã khởi động thành công, bạn có thể truy cập:

-   **Admin Frontend**: `http://localhost:8081`
-   **Backend API (Swagger)**: `http://localhost:8080/swagger` (Backend API chạy trên cổng 8080)

**Lưu ý:**
*   Lần đầu tiên chạy có thể mất một chút thời gian để tải xuống các image Docker và build ứng dụng.
*   Các cấu hình nhạy cảm (ví dụ: chuỗi kết nối database, khóa API) được quản lý thông qua tệp `infra/docker-compose.yml` khi sử dụng Docker Compose. Các tệp `.env` trong các thư mục `apps/backend` và `apps/admin` chỉ được sử dụng khi chạy các ứng dụng riêng lẻ mà không thông qua Docker Compose.

### 2.3. Chạy riêng lẻ

#### Backend

    Đảm bảo bạn có một instance MySQL đang chạy. Bạn có thể sử dụng instance từ tệp `infra/docker-compose.yml`:
    ```bash
    docker-compose -f src/infra/docker-compose.yml up -d mysql
    ```
2.  **Cấu hình Backend**: Các cấu hình cho Backend (chuỗi kết nối database, JWT, AI, Vector Store, Storage, v.v.) được quản lý thông qua các biến môi trường của hệ thống hoặc tệp `.env` cục bộ. Nếu bạn đang chạy riêng lẻ, hãy đảm bảo các biến môi trường cần thiết đã được thiết lập.

3.  **Chạy backend**: 
    ```bash
    cd apps/backend
    dotnet run --project apps/backend/src/Web
    ```
    Khi chạy backend ở chế độ Development, hệ thống sẽ tự động áp dụng các migrations và seed dữ liệu mẫu (nếu database trống).

#### Admin Frontend

1.  Điều hướng đến thư mục `admin`:
    ```bash
    cd apps/admin
    ```
2.  Cài đặt các dependency:
    ```bash
    npm install
    ```
3.  **Cấu hình Frontend**: Các biến môi trường cho Frontend (ví dụ: API Base URL, cấu hình Auth0) được quản lý thông qua các biến môi trường của hệ thống hoặc tệp `.env` cục bộ. Nếu bạn đang chạy riêng lẻ, hãy đảm bảo các biến môi trường cần thiết đã được thiết lập.

4.  Chạy Frontend ở chế độ phát triển:
    ```bash
    npm run dev
    ```
    Frontend sẽ chạy trên `http://localhost:5173` (hoặc một cổng khác nếu 5173 đã được sử dụng).

## 3. Build và Deploy

### 3.1. Build Docker images

```bash
# Build Backend
docker build -t family-tree-backend -f infra/Dockerfile.backend .

# Build Admin Frontend
docker build -t family-tree-admin -f infra/Dockerfile.admin .
```

### 3.2. Deploy

Dự án được thiết kế để deploy dễ dàng bằng Docker Compose. Trên server, bạn chỉ cần chạy:

```bash
docker-compose -f infra/docker-compose.yml up -d
```

## 4. Quản lý Database

Dự án sử dụng Entity Framework Core Migrations để quản lý schema database. Các lệnh này cần được chạy từ thư mục gốc của project `backend`.

**Lưu ý quan trọng:** Kể từ khi loại bỏ ASP.NET Core Identity, việc quản lý người dùng và vai trò (user/role) không còn được thực hiện qua database cục bộ nữa mà hoàn toàn do nhà cung cấp JWT (ví dụ: Auth0) đảm nhiệm. Database chỉ chứa dữ liệu nghiệp vụ của ứng dụng.

### 4.1. Tạo migration mới

Khi bạn thay đổi các Entity trong Domain Layer, bạn cần tạo một migration mới để cập nhật schema database. 

```bash
dotnet ef migrations add [MigrationName] --project apps/backend/src/Infrastructure --startup-project apps/backend/src/Web
```

*   Thay thế `[MigrationName]` bằng một tên có ý nghĩa (ví dụ: `AddFamilyAddressField`).

### 4.2. Áp dụng migration

Sau khi tạo migration, bạn cần áp dụng nó vào database để các thay đổi schema có hiệu lực.

```bash
dotnet ef database update --project apps/backend/src/Infrastructure --startup-project apps/backend/src/Web
```

**Lưu ý:**

*   Để biết hướng dẫn chi tiết hơn về cách quản lý database migrations, bao gồm cả việc tạo migration ban đầu, vui lòng tham khảo [Hướng dẫn Backend](./backend-guide.md#9-database-migration).
*   Khi chạy Backend ở chế độ Development, `ApplicationDbContextInitialiser` sẽ tự động gọi `database update` nếu database là relational và chưa được cập nhật.

## 5. Seeding a Database

Dự án được cấu hình để tự động seed dữ liệu mẫu khi Backend khởi động ở chế độ Development, nếu database chưa có dữ liệu. Cơ chế này được triển khai trong `ApplicationDbContextInitialiser`.

#### Luồng hoạt động

1.  Đảm bảo `ASPNETCORE_ENVIRONMENT` được đặt là `Development`.
2.  Đảm bảo `UseInMemoryDatabase` trong `appsettings.Development.json` được đặt là `false` (nếu bạn muốn seed vào MySQL) hoặc `true` (nếu bạn muốn seed vào In-Memory DB).
3.  Khởi động Backend (ví dụ: `dotnet run --project src/Web`).

Backend sẽ kiểm tra xem database đã có dữ liệu chưa (ví dụ: bảng `Families` có trống không). Nếu trống, nó sẽ chạy phương thức `SeedAsync()` trong `ApplicationDbContextInitialiser` để điền dữ liệu mẫu (ví dụ: dữ liệu về Royal Family).

**Lưu ý:** Nếu bạn đã có dữ liệu trong database, quá trình seeding sẽ bị bỏ qua để tránh ghi đè dữ liệu hiện có.

## 6. Sử dụng Dịch vụ Phân quyền (Authorization Service)

Hệ thống cung cấp một dịch vụ phân quyền tập trung (`IAuthorizationService`) để kiểm tra quyền hạn của người dùng trong các ngữ cảnh khác nhau, bao gồm cả vai trò toàn cục và vai trò cụ thể theo gia đình. Các nhà phát triển nên sử dụng dịch vụ này để thực hiện các kiểm tra quyền trong logic nghiệp vụ và các endpoint API.

### 6.1. Inject `IAuthorizationService`

Bạn có thể inject `IAuthorizationService` vào các service, handler hoặc controller của mình thông qua Dependency Injection.

```csharp
// Ví dụ trong một Application Service hoặc Controller
public class FamilyService : IFamilyService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IApplicationDbContext _context;

    public FamilyService(IAuthorizationService authorizationService, IApplicationDbContext context)
    {
        _authorizationService = authorizationService;
        _context = context;
    }

    public async Task<Result<bool>> UpdateFamily(Guid familyId, UpdateFamilyCommand command, CancellationToken cancellationToken)
    {
        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null || string.IsNullOrEmpty(currentUserProfile.ExternalId))
        {
            return Result<bool>.Failure("User profile or ExternalId not found.", 404);
        }

        // Kiểm tra quyền quản lý gia đình
        if (!_authorizationService.CanManageFamily(familyId, currentUserProfile))
        {
            return Result<bool>.Failure("User does not have permission to manage this family.", 403);
        }

        // Thực hiện logic cập nhật gia đình...
        var family = await _context.Families.FindAsync(familyId);
        if (family == null)
        {
            return Result<bool>.Failure("Family not found.", 404);
        }

        family.Name = command.Name;
        // ... cập nhật các thuộc tính khác

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
```

### 6.2. Các phương thức kiểm tra quyền

`IAuthorizationService` cung cấp các phương thức sau để kiểm tra quyền:

*   `IsAdmin()`: Kiểm tra xem người dùng hiện tại có vai trò `Administrator` toàn cục hay không.
*   `GetCurrentUserProfileAsync()`: Lấy thông tin `UserProfile` của người dùng hiện tại, bao gồm cả `ExternalId`.
*   `CanAccessFamily(Guid familyId, UserProfile userProfile)`: Kiểm tra xem người dùng có quyền truy cập (ít nhất là `Viewer`) vào một gia đình cụ thể hay không.
*   `CanManageFamily(Guid familyId, UserProfile userProfile)`: Kiểm tra xem người dùng có quyền quản lý (vai trò `Manager`) đối với một gia đình cụ thể hay không.
*   `HasFamilyRole(Guid familyId, UserProfile userProfile, FamilyRole requiredRole)`: Kiểm tra xem người dùng có vai trò `requiredRole` hoặc cao hơn trong một gia đình cụ thể hay không.

### 6.3. Lưu ý khi phát triển

*   **Luôn kiểm tra quyền**: Đảm bảo rằng mọi hành động nhạy cảm hoặc truy cập dữ liệu đều được kiểm tra quyền một cách thích hợp.
*   **Sử dụng `FamilyRole`**: Khi làm việc với các chức năng liên quan đến gia đình, hãy sử dụng `FamilyRole` enum để định nghĩa và kiểm tra các cấp độ quyền hạn.
*   **Tách biệt trách nhiệm**: Giữ logic kiểm tra quyền trong `IAuthorizationService` hoặc các chính sách ủy quyền để đảm bảo tính nhất quán và dễ bảo trì.





## 7. Sử dụng AutoMapper cho DTO Mapping

Dự án sử dụng AutoMapper để tự động ánh xạ dữ liệu giữa các đối tượng (ví dụ: từ Entity sang DTO và ngược lại). Điều này giúp giảm thiểu mã lặp lại và giữ cho các lớp rõ ràng hơn.

#### Cấu hình Mapping

Các cấu hình mapping được định nghĩa trong các lớp kế thừa từ `Profile` của AutoMapper, ví dụ như `MappingProfile.cs` trong thư mục `Application/Common/Mappings`.

```csharp
// apps/backend/src/Application/Common/Mappings/MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Family, FamilyDto>();
        CreateMap<Member, MemberDto>();
        // ... các mappings khác
    }
}
```

#### Inject và Sử dụng `IMapper`

Bạn có thể inject `IMapper` vào các service, handler hoặc controller của mình và sử dụng nó để thực hiện ánh xạ.

```csharp
// Ví dụ trong một Application Handler
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
        if (family == null)
        {
            return Result<FamilyDto>.Failure("Family not found.", 404);
        }
        return Result<FamilyDto>.Success(_mapper.Map<FamilyDto>(family));
    }
}
```
