# Hướng dẫn cho Developer

## 1. Giới thiệu
Hướng dẫn này cung cấp các chỉ dẫn để thiết lập môi trường phát triển, chạy dự án và đóng góp vào dự án Cây Gia Phả.

**Tổng quan dự án:**
-   **Backend:** .NET 8, ASP.NET Core, Entity Framework Core, MySQL
-   **Frontend:** Vue.js 3, Vite, Vuetify, Pinia
-   **Cơ sở dữ liệu:** MySQL 8.0
-   **Containerization:** Docker

## 2. Yêu cầu cần thiết
-   [Docker & Docker Compose](https://docs.docker.com/get-docker/)
-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Node.js 20+](https://nodejs.org/en/download/)

## 3. Bắt đầu

### 3.1. Clone Repository
```bash
git clone https://github.com/hkthao/family-tree-3.git
cd family-tree-3
```

### 3.2. Cấu hình
Dự án sử dụng các biến môi trường để cấu hình.

-   **Backend:** Cấu hình backend được quản lý trong `backend/src/Web/appsettings.json` và có thể được ghi đè bởi các biến môi trường. Tệp `docker-compose.yml` thiết lập chuỗi kết nối cho cơ sở dữ liệu.
-   **Frontend:** Cấu hình frontend được quản lý thông qua các tệp `.env` trong thư mục `frontend`. Điểm cuối API được cấu hình trong các tệp này.

## 4. Chạy ứng dụng

### 4.1. Sử dụng Docker Compose (Khuyến nghị)
Đây là cách dễ nhất để chạy toàn bộ ứng dụng.

1.  Đảm bảo Docker Desktop đang chạy.
2.  Trong thư mục gốc của dự án, chạy:
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```
3.  Truy cập ứng dụng:
    -   **Frontend:** `http://localhost`
    -   **Backend API (Swagger UI):** `http://localhost:8080/swagger`

### 4.2. Chạy Backend và Frontend riêng lẻ

#### 4.2.1. Backend
1.  Đảm bảo bạn có một instance MySQL đang chạy. Bạn có thể sử dụng instance từ tệp `docker-compose.yml`:
    ```bash
    docker-compose -f infra/docker-compose.yml up -d mysql
    ```
2.  Thiết lập chuỗi kết nối cơ sở dữ liệu. Bạn có thể sử dụng một biến môi trường hoặc cập nhật `backend/src/Web/appsettings.Development.json`.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Port=3306;Database=familytree_db;Uid=root;Pwd=root_password;"
    }
    ```
3.  Chạy backend:
    ```bash
    cd backend
    dotnet run --project src/Web
    ```

#### 4.2.2. Frontend
1.  Điều hướng đến thư mục `frontend`:
    ```bash
    cd frontend
    ```
2.  Cài đặt các dependency:
    ```bash
    npm install
    ```
3.  Chạy máy chủ phát triển:
    ```bash
    npm run dev
    ```
4.  Frontend sẽ có sẵn tại `http://localhost:5173` (hoặc một cổng khác nếu 5173 đang được sử dụng).

## 5. Quy trình phát triển

### 5.1. Backend

#### 5.1.1. Chạy Tests
Để chạy tất cả các unit test cho backend, chạy lệnh sau từ thư mục gốc:
```bash
dotnet test backend/backend.sln
```

#### 5.1.2. Chạy Tests với Coverage
Để chạy test và tạo báo cáo coverage, sử dụng lệnh sau từ thư mục gốc:
```bash
dotnet test backend/backend.sln /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./backend/artifacts/coverage/coverage.cobertura.xml
```
Lệnh này sẽ tạo ra một tệp `coverage.cobertura.xml`. Để xem báo cáo ở định dạng HTML, bạn có thể sử dụng `reportgenerator`:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:./backend/artifacts/coverage/coverage.cobertura.xml" "-targetdir:./backend/coverage-report" -reporttypes:Html
```
Báo cáo sẽ có sẵn trong thư mục `backend/coverage-report`.

#### 5.1.3. Code Style
Để kiểm tra các vấn đề về định dạng:
```bash
dotnet format backend/ --verify-no-changes --include-generated
```
Để tự động định dạng code:
```bash
dotnet format backend/ --include-generated
```

#### 5.1.4. Database Migrations
Dự án sử dụng Entity Framework Core Migrations để quản lý các thay đổi schema của cơ sở dữ liệu.

-   **Để tạo một migration mới:**
    ```bash
    dotnet ef migrations add <MigrationName> --project backend/src/Infrastructure --startup-project backend/src/Web
    ```
-   **Để áp dụng các migration vào cơ sở dữ liệu:**
    ```bash
    dotnet ef database update --project backend/src/Infrastructure --startup-project backend/src/Web
    ```

### 5.2. Frontend

#### 5.2.1. Chạy Tests
Để chạy tất cả các unit và component test cho frontend:
```bash
npm run test:unit --prefix frontend
```

#### 5.2.2. Chạy Tests với Coverage
Để chạy test và kiểm tra ngưỡng coverage 80%:
```bash
npm run test:coverage --prefix frontend
```

#### 5.2.3. Code Style và Linting
Để kiểm tra các vấn đề về linting:
```bash
npm run lint --prefix frontend
```
Để tự động sửa các vấn đề về linting:
```bash
npm run lint:fix --prefix frontend
```

## 6. Hướng dẫn đóng góp

### 6.1. Chiến lược Branch
-   `main`: Branch ổn định cho code sẵn sàng sản xuất.
-   `develop`: Branch phát triển chính.
-   `feature/<feature-name>`: Cho các tính năng mới.
-   `bugfix/<bug-name>`: Cho các bản sửa lỗi.
-   `docs/<docs-name>`: Cho các cập nhật tài liệu.
-   `hotfix/<fix-name>`: Cho các bản sửa lỗi khẩn cấp trên production.

### 6.2. Commit Messages
Dự án này tuân theo đặc tả [Conventional Commits](https://www.conventionalcommits.org/).

### 6.3. Quy trình Pull Request
-   Đảm bảo code của bạn được định dạng và lint.
-   Tất cả các test phải qua.
-   Test coverage phải đạt ngưỡng yêu cầu (>=80%).
-   Cập nhật tài liệu nếu cần.
-   Sử dụng tiêu đề mô tả và cung cấp mô tả rõ ràng về các thay đổi.

## 7. CI/CD
Pipeline CI/CD được định nghĩa trong `.github/workflows/ci.yml`. Nó bao gồm các bước sau:
-   Build và test backend và frontend.
-   Kiểm tra các vấn đề về định dạng và linting.
-   Tạo và tải lên các báo cáo coverage.
-   Build các Docker image.

## 8. Seeding a Database
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
