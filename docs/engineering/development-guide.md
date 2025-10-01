# Hướng dẫn Phát triển

Tài liệu này hướng dẫn cách cài đặt môi trường, build, và deploy dự án Cây Gia Phả.

## 1. Yêu cầu môi trường

-   **Docker và Docker Compose**: Để chạy ứng dụng trong môi trường container.
-   **.NET 8 SDK**: Cho việc phát triển Backend.
-   **Node.js 20+**: Cho việc phát triển Frontend.

## 2. Cài đặt và Chạy dự án

### 2.1. Clone repository

```bash
git clone https://github.com/your-username/family-tree-3.git
cd family-tree-3
```

### 2.2. Chạy với Docker Compose (Khuyến nghị)

Đây là cách nhanh nhất để chạy cả Frontend, Backend, và Database.

```bash
docker-compose -f infra/docker-compose.yml up --build
```

Sau khi chạy lệnh trên, bạn có thể truy cập:

-   **Frontend**: `http://localhost`
-   **Backend API (Swagger)**: `http://localhost:8080/swagger`

### 2.3. Chạy riêng lẻ

#### Backend

1.  Đảm bảo bạn có một instance MySQL đang chạy. Bạn có thể sử dụng instance từ tệp `docker-compose.yml`:
    ```bash
    docker-compose -f infra/docker-compose.yml up -d mysql
    ```
2.  Thiết lập chuỗi kết nối cơ sở dữ liệu trong `backend/src/Web/appsettings.Development.json`.
3.  Chạy backend:
    ```bash
    cd backend
    dotnet run --project src/Web
    ```

#### Frontend

```bash
cd frontend
npm install
npm run dev
```

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

Dự án sử dụng Entity Framework Core Migrations để quản lý schema database.

### Tạo migration mới

```bash
dotnet ef migrations add [MigrationName] --project backend/src/Infrastructure --startup-project backend/src/Web
```

### Áp dụng migration

```bash
dotnet ef database update --project backend/src/Infrastructure --startup-project backend/src/Web
```

## 5. Seeding a Database
Để điền dữ liệu mẫu vào cơ sở dữ liệu:
1.  Đảm bảo container MySQL đang chạy.
2.  Điều hướng đến thư mục `infra/seeds`.
3.  Cài đặt các dependency:
    ```bash
    npm install
    ```
4.  Chạy seed script:
    ```bash
    npm run seed
    ```