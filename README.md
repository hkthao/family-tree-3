# Dự án Cây Gia Phả (Family Tree Project)

![CI Workflow Status](https://github.com/hkthao/family-tree-3/actions/workflows/ci.yml/badge.svg)

## 1. 🏷️ Thông tin tổng quan (Overview)

Ứng dụng quản lý cây gia phả giúp người dùng tạo, xem và chia sẻ sơ đồ gia đình một cách dễ dàng và chuyên nghiệp.

**Công nghệ chính:**
*   **Backend:** .NET 8, Clean Architecture, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation, JWT Authentication, Novu
*   **Frontend:** Vue.js 3, TypeScript, Vite, Vuetify 3, Pinia, Vue Router, Axios, ESLint, Prettier
*   **Cơ sở dữ liệu:** MySQL
*   **Triển khai:** Docker, Docker Compose, Nginx
*   **CI/CD:** GitHub Actions

## 2. 🏗️ Kiến trúc hệ thống (Architecture)

Dự án được tổ chức theo kiến trúc phân lớp rõ ràng để dễ dàng phát triển và bảo trì:

*   `src/backend`: Chứa mã nguồn cho dịch vụ API backend, tuân thủ Clean Architecture với các mẫu thiết kế như DDD (Domain-Driven Design) và CQRS (Command Query Responsibility Segregation) sử dụng MediatR. Tương tác với cơ sở dữ liệu thông qua Entity Framework Core.
*   `src/frontend`: Chứa mã nguồn cho giao diện người dùng, được xây dựng với Vue.js 3, TypeScript và Vite.
*   `src/infra`: Chứa các tệp cấu hình hạ tầng như Dockerfile cho backend và frontend, Docker Compose để điều phối các dịch vụ.
*   `.github/workflows/ci.yml`: Định nghĩa pipeline CI/CD, tự động hóa quá trình build, test và linting.

## 3. ⚙️ Cách cài đặt và chạy (Setup & Run Locally)

### 🚀 Yêu cầu:

*   **Docker & Docker Compose**: Phiên bản mới nhất (khuyến nghị để chạy toàn bộ ứng dụng).
*   **.NET 8 SDK**: Phiên bản 8.0.x (hoặc mới hơn, cần cho phát triển backend).
*   **Node.js >= 20**: Phiên bản 20.x (hoặc mới hơn, cần cho phát triển frontend).
*   **Công cụ CLI**: `dotnet-ef` để quản lý Entity Framework Core migrations (cài đặt bằng `dotnet tool install --global dotnet-ef`).

### 🧩 Cách chạy backend (riêng lẻ):

```bash
cd src/backend
dotnet restore
dotnet build
dotnet run --project src/Web
```
API sẽ khả dụng tại `http://localhost:5000` và Swagger UI tại `http://localhost:5000/swagger`.

### 💻 Cách chạy frontend (riêng lẻ):

```bash
cd src/frontend
npm install
npm run dev
```
Ứng dụng sẽ chạy trên `http://localhost:5173`.

### 🐳 Chạy bằng Docker Compose (cả hai service):

Đây là cách nhanh nhất và được khuyến nghị để chạy cả Frontend, Backend, và Database trong môi trường phát triển.

1.  **Cấu hình biến môi trường**: Tạo tệp `.env` trong thư mục `src/backend` và `src/frontend` dựa trên các tệp `.env.example` tương ứng. Cấu hình các biến môi trường cần thiết như chuỗi kết nối cơ sở dữ liệu, thông tin Auth0 (Domain, Client ID, Audience), và các khóa API khác.
2.  **Chạy Docker Compose:**
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```
    Sau khi các dịch vụ khởi động, bạn có thể truy cập:
    *   **Frontend:** [http://localhost](http://localhost)
    *   **Backend API (Swagger):** [http://localhost:5000/swagger](http://localhost:5000/swagger)

3.  **Cấu hình Database (chỉ lần đầu)**:
    Nếu bạn chạy Backend với MySQL (không phải In-Memory Database), bạn cần áp dụng migrations để tạo schema database và seed dữ liệu mẫu. Khi chạy ở chế độ Development, ứng dụng sẽ tự động áp dụng migrations và seed dữ liệu nếu database trống.
    ```bash
    dotnet ef database update --project src/backend/src/Infrastructure --startup-project src/backend/src/Web
    ```

## 4. 🧪 Chạy kiểm thử (Testing)

### Backend:

```bash
cd src/backend
dotnet test
```

### Frontend:

```bash
cd src/frontend
npm run test:coverage
```

CI/CD tự động thực hiện các bước kiểm thử này trong workflow `.github/workflows/ci.yml`.

## 5. 🔄 CI/CD Pipeline

Dự án sử dụng GitHub Actions để tự động hóa quy trình CI/CD.

*   **Workflow CI (`.github/workflows/ci.yml`)**:
    *   Được kích hoạt khi có `push` hoặc `pull_request` nhắm vào nhánh `main`.
    *   Thực hiện:
        1.  Build và chạy unit tests cho backend.
        2.  Build, lint và chạy unit tests cho frontend.
        3.  Build Docker image cho cả backend và frontend.
        4.  Upload các Docker image này dưới dạng artifact.

*   **Workflow CD (`.github/workflows/cd.yml`)**:
    *   Được kích hoạt khi workflow CI hoàn thành thành công trên nhánh `main`.
    *   Tải xuống các Docker image artifact.
    *   Đăng nhập vào Docker Hub.
    *   Push các Docker image lên Docker Hub.

## 6. 📁 Cấu trúc thư mục (Project Structure)

```
family-tree-3/
├── .github/workflows/ci.yml
├── src/
│   ├── backend/
│   ├── frontend/
│   └── infra/
│       ├── Dockerfile.backend
│       └── Dockerfile.frontend
└── README.md
```

## 7. 🧭 Tài liệu chi tiết (References)

Để có thông tin chi tiết hơn về từng phần của dự án, vui lòng tham khảo các tài liệu sau:

*   [**Kiến trúc tổng quan**](./docs/engineering/architecture.md)
*   [**Hướng dẫn Backend**](./docs/engineering/backend-guide.md)
*   [**Hướng dẫn Frontend**](./docs/engineering/frontend-guide.md)
*   [**Tham chiếu API**](./docs/engineering/api-reference.md)
*   [**Mô hình Dữ liệu**](./docs/engineering/data-model.md)

## 8. 👥 Đóng góp (Contributing)

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [Hướng dẫn Đóng góp](./docs/engineering/contribution-guide.md) của chúng tôi để biết chi tiết về quy tắc ứng xử, quy trình gửi pull request, và các tiêu chuẩn về code style (`dotnet format`, `eslint`, Prettier).

## 9. 📜 Giấy phép (License)

Dự án này được cấp phép theo Giấy phép MIT. Xem tệp [LICENSE](./LICENSE) để biết chi tiết.
