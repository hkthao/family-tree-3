# Dự án Dòng Họ Việt (Family Tree Project)

![CI Workflow Status](https://github.com/hkthao/family-tree-3/actions/workflows/ci.yml/badge.svg)

## 1. 🏷️ Thông tin tổng quan (Overview)

Ứng dụng quản lý Dòng Họ Việt giúp người dùng tạo, xem và chia sẻ sơ đồ gia đình một cách dễ dàng và chuyên nghiệp.

**Công nghệ chính:**
*   **Backend:** .NET 8, Clean Architecture, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation, JWT Authentication, Novu
*   **Frontend (Admin)::** Vue.js 3, TypeScript, Vite, Vuetify 3, Pinia, Vue Router, Axios, ESLint, Prettier
*   **Cơ sở dữ liệu:** MySQL
*   **Triển khai:** Docker, Docker Compose, Nginx
*   **CI/CD:** GitHub Actions

## 2. 🏗️ Kiến trúc hệ thống (Architecture)

Dự án được tổ chức theo kiến trúc monorepo, phân chia thành các phần chính để dễ dàng phát triển và bảo trì:

*   **`apps/`**: Chứa các ứng dụng chính có thể chạy độc lập.
    *   `apps/backend`: Mã nguồn cho dịch vụ API backend, tuân thủ Clean Architecture với các mẫu thiết kế như DDD (Domain-Driven Design) và CQRS (Command Query Responsibility Segregation) sử dụng MediatR. Tương tác với cơ sở dữ liệu thông qua Entity Framework Core.
    *   `apps/admin`: Mã nguồn cho giao diện quản trị, được xây dựng với Vue.js 3, TypeScript và Vite.
*   **`services/`**: Chứa các dịch vụ phụ trợ.
    *   `services/face-service`: Dịch vụ xử lý khuôn mặt bằng Python.
*   **`packages/`**: Chứa các gói mã nguồn được chia sẻ giữa các ứng dụng (ví dụ: `shared-types` cho các định nghĩa TypeScript dùng chung).
*   **`infra/`**: Chứa các tệp cấu hình hạ tầng như Dockerfile cho từng ứng dụng, Docker Compose để điều phối các dịch vụ.
*   **`.github/workflows/`**: Định nghĩa các pipeline CI/CD, tự động hóa quá trình build, test và linting cho từng ứng dụng.

## 3. ⚙️ Cách cài đặt và chạy (Setup & Run Locally)

### 🚀 Yêu cầu:

*   **Docker & Docker Compose**: Phiên bản mới nhất (khuyến nghị để chạy toàn bộ ứng dụng).
*   **.NET 8 SDK**: Phiên bản 8.0.x (hoặc mới hơn, cần cho phát triển backend).
*   **Node.js >= 20**: Phiên bản 20.x (hoặc mới hơn, cần cho phát triển frontend).
*   **Công cụ CLI**: `dotnet-ef` để quản lý Entity Framework Core migrations (cài đặt bằng `dotnet tool install --global dotnet-ef`).

### 🧩 Cách chạy backend (riêng lẻ):

```bash
cd apps/backend
dotnet restore
dotnet build
dotnet run --project src/Web
```
API sẽ khả dụng tại `http://localhost:8080` và Swagger UI tại `http://localhost:8080/swagger`.

### 💻 Cách chạy admin frontend (riêng lẻ):

```bash
cd apps/admin
npm install
npm run dev
```
Ứng dụng sẽ chạy trên `http://localhost:5173`.


### 🐳 Chạy bằng Docker Compose (tất cả các service):

Đây là cách nhanh nhất và được khuyến nghị để chạy tất cả các ứng dụng và Database trong môi trường phát triển.

1.  **Cấu hình biến môi trường**: Khi chạy với Docker Compose, các biến môi trường được định nghĩa trực tiếp trong tệp `infra/docker-compose.yml`. Các tệp `.env` trong các thư mục `apps/backend` và `apps/admin` chỉ được sử dụng khi chạy các ứng dụng riêng lẻ mà không thông qua Docker Compose.
    *   `REPLICATE_API_TOKEN`: Required for the `image-restoration-service`. Obtain your API token from [Replicate](https://replicate.com/account).
2.  **Chạy Docker Compose:**
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```
    Sau khi các dịch vụ khởi động, bạn có thể truy cập:
    *   **Admin Frontend:** [http://localhost:8081](http://localhost:8081)
    *   **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

3.  **Cấu hình Database (chỉ lần đầu)**:
    Nếu bạn chạy Backend với MySQL (không phải In-Memory Database), bạn cần áp dụng migrations để tạo schema database và seed dữ liệu mẫu. Khi chạy ở chế độ Development, ứng dụng sẽ tự động áp dụng migrations và seed dữ liệu nếu database trống.
    ```bash
    dotnet ef database update --project apps/backend/src/Infrastructure --startup-project apps/backend/src/Web
    ```

## 4. 🧪 Chạy kiểm thử (Testing)

### Backend:

```bash
cd apps/backend
dotnet test
```

### Admin Frontend:

```bash
cd apps/admin
npm run test:coverage
```

CI/CD tự động thực hiện các bước kiểm thử này trong workflow `.github/workflows/ci.yml`.

## 5. 🔄 CI/CD Pipeline

Dự án sử dụng GitHub Actions để tự động hóa quy trình CI/CD.

*   **Workflow CI (`.github/workflows/ci.yml`)**:
    *   Được kích hoạt khi có `push` hoặc `pull_request` nhắm vào nhánh `main`.
    *   Thực hiện build, test và lint cho tất cả các ứng dụng (`backend`, `admin`, `face-service`).

*   **Workflows CD (`.github/workflows/cd-*.yml`)**:
    *   Được kích hoạt khi workflow CI hoàn thành thành công trên nhánh `main`.
    *   Tải xuống các Docker image artifact.
    *   Đăng nhập vào Docker Hub.
    *   Build và Push các Docker image riêng biệt cho `backend`, `admin` và `face-service` lên Docker Hub.

## 6. 📁 Cấu trúc thư mục (Project Structure)

```
family-tree-3/
├── apps/
│   ├── admin/         # Giao diện quản trị (Vue + Vuetify)
│   └── backend/       # API Backend (ASP.NET Core)
├── services/
│   └── face-service/  # Dịch vụ xử lý khuôn mặt (Python)
├── packages/
│   ├── shared-types/  # Nơi chia sẻ các Types/DTOs giữa frontend và backend
│   └── ui-components/ # (Tùy chọn) Nơi chia sẻ các component UI giữa `admin` và `public`
├── infra/
│   ├── docker-compose.yml
│   ├── docker-compose.prod.yml
│   ├── nginx/
│   └── services/
│       └── face-service/
├── .github/workflows/ # Các workflow CI/CD
├── docs/              # Tài liệu dự án
├── tests/             # Các bài kiểm thử tổng thể
├── .gitignore
├── CODE_OF_CONDUCT.md
├── GEMINI.md
├── LICENSE
├── omnisharp.json
├── package.json
├── PULL_REQUEST_TEMPLATE.md
├── README.md
└── .config/
```

## 7. 🧭 Tài liệu chi tiết (References)

Để có thông tin chi tiết hơn về từng phần của dự án, vui lòng tham khảo các tài liệu sau:

*   [**Kiến trúc tổng quan**](./docs/engineering/architecture.md)
*   [**Hướng dẫn Backend**](./docs/engineering/backend-guide.md)
*   [**Hướng dẫn Frontend (Admin)**](./docs/engineering/frontend-guide.md)
*   [**Tham chiếu API**](./docs/engineering/api-reference.md)
*   [**Mô hình Dữ liệu**](./docs/engineering/data-model.md)

*   [**Cơ sở Kiến thức AI cho Chat Assistant**](./docs/engineering/ai-chat-assistant-kb.md)
*   [**Đội ngũ Phát triển**](./docs/project/team.md)

## 8. 👥 Đóng góp (Contributing)

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [Hướng dẫn Đóng góp](./docs/engineering/contribution-guide.md) của chúng tôi để biết chi tiết về quy tắc ứng xử, quy trình gửi pull request, và các tiêu chuẩn về code style (`dotnet format` cho backend, `eslint` cho frontend).

## 9. 📜 Giấy phép (License)

Dự án này được cấp phép theo Giấy phép MIT. Xem tệp [LICENSE](./LICENSE) để biết chi tiết.

## 10. ❤️ Ủng hộ (Support)

Nếu bạn thấy dự án này hữu ích hoặc muốn ủng hộ công sức phát triển, bạn có thể:

*   **Buy me a coffee**: buymeacoffee.com/thaohk90e
*   **Momo**: 0946351139

Mọi sự ủng hộ đều là động lực lớn để chúng tôi tiếp tục phát triển và cải thiện dự án!