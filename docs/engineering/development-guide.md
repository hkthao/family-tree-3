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

---

Tài liệu này hướng dẫn cách cài đặt môi trường, build, và deploy dự án Cây Gia Phả.

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

-   **Frontend**: `http://localhost` (Nginx sẽ phục vụ Frontend)
-   **Backend API (Swagger)**: `http://localhost:8080/swagger` (Backend API chạy trên cổng 8080)

**Lưu ý:** Lần đầu tiên chạy có thể mất một chút thời gian để tải xuống các image Docker và build ứng dụng.

### 2.3. Chạy riêng lẻ

#### Backend

1.  Đảm bảo bạn có một instance MySQL đang chạy. Bạn có thể sử dụng instance từ tệp `infra/docker-compose.yml`:
    ```bash
    docker-compose -f infra/docker-compose.yml up -d mysql
    ```
2.  **Cấu hình Backend**: 
    *   **Database**: Chỉnh sửa `backend/src/Web/appsettings.Development.json` để đảm bảo `UseInMemoryDatabase` là `false` và chuỗi kết nối `DefaultConnection` trỏ đến `localhost`.
    *   **Auth0**: Cấu hình Auth0 Domain và Audience trong `backend/src/Web/Properties/launchSettings.json` cho profile `backend.Web`.

    ```json
    // backend/src/Web/appsettings.Development.json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Port=3306;Database=familytree_db;Uid=root;Pwd=root_password;"
      },
      "UseInMemoryDatabase": false,
      // ... các cấu hình khác
    }
    ```

    ```json
    // backend/src/Web/Properties/launchSettings.json
    {
      "profiles": {
        "backend.Web": {
          // ...
          "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development",
            "Auth0:Domain": "YOUR_AUTH0_DOMAIN", // Thay bằng Auth0 Domain của bạn
            "Auth0:Audience": "YOUR_AUTH0_AUDIENCE" // Thay bằng Auth0 Audience của bạn
          }
        }
      }
    }
    ```

3.  **Chạy backend**: 
    ```bash
    cd backend
    dotnet run --project src/Web
    ```
    Khi chạy backend ở chế độ Development, hệ thống sẽ tự động áp dụng các migrations và seed dữ liệu mẫu (nếu database trống).

#### Frontend

1.  Điều hướng đến thư mục `frontend`:
    ```bash
    cd frontend
    ```
2.  Cài đặt các dependency:
    ```bash
    npm install
    ```
3.  **Cấu hình Frontend**: 
    *   **Biến môi trường**: Tạo file `.env.development` trong thư mục `frontend` dựa trên `frontend/.env.example`. File này chứa các biến môi trường cho Auth0 và API Base URL.
        ```
        # frontend/.env.example
        VITE_USE_MOCK=false
        VITE_AUTH0_DOMAIN="YOUR_AUTH0_DOMAIN"
        VITE_AUTH0_CLIENT_ID="YOUR_AUTH0_CLIENT_ID"
        VITE_AUTH0_AUDIENCE="YOUR_AUTH0_AUDIENCE"
        VITE_API_BASE_URL="/api"
        ```
        Bạn cần tạo file `frontend/.env.development` và điền các giá trị thực tế của bạn. Ví dụ:
        ```
        # frontend/.env.development
        VITE_USE_MOCK=false
        VITE_AUTH0_DOMAIN="https://dev-g76tq00gicwdzk3z.us.auth0.com"
        VITE_AUTH0_CLIENT_ID="v4jSe5QR4Uj6ddoBBMHNtaDNHwv8UzQN"
        VITE_AUTH0_AUDIENCE="http://localhost:5000"
        VITE_API_BASE_URL="/api"
        ```
    *   **Vite Proxy**: Đảm bảo `frontend/vite.config.ts` được cấu hình đúng để proxy các yêu cầu API đến Backend đang chạy. Ví dụ:

    ```typescript
    // frontend/vite.config.ts
    export default defineConfig({
      server: {
        proxy: {
          '/api': {
            target: 'http://localhost:5000', // Hoặc cổng Backend đang chạy
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            // secure: false, // Nếu Backend chạy HTTPS với chứng chỉ tự ký
          },
        },
      },
    });
    ```

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

# Build Frontend
docker build -t family-tree-frontend -f infra/Dockerfile.frontend .
```

### 3.2. Deploy

Dự án được thiết kế để deploy dễ dàng bằng Docker Compose. Trên server, bạn chỉ cần chạy:

```bash
docker-compose -f infra/docker-compose.yml up -d
```

## 4. Quản lý Database

Dự án sử dụng Entity Framework Core Migrations để quản lý schema database. Các lệnh này cần được chạy từ thư mục gốc của project `backend`.

**Lưu ý quan trọng:** Kể từ khi loại bỏ ASP.NET Core Identity, việc quản lý người dùng và vai trò (user/role) không còn được thực hiện qua database cục bộ nữa mà hoàn toàn do Auth0 đảm nhiệm. Database chỉ chứa dữ liệu nghiệp vụ của ứng dụng.

### 4.1. Tạo migration mới

Khi bạn thay đổi các Entity trong Domain Layer, bạn cần tạo một migration mới để cập nhật schema database. 

```bash
dotnet ef migrations add [MigrationName] --project src/Infrastructure --startup-project src/Web
```

*   Thay thế `[MigrationName]` bằng một tên có ý nghĩa (ví dụ: `AddFamilyAddressField`).

### 4.2. Áp dụng migration

Sau khi tạo migration, bạn cần áp dụng nó vào database để các thay đổi schema có hiệu lực.

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
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
