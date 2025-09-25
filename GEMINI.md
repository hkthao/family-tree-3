# Tóm tắt Kho lưu trữ Dự án Cây Gia Phả

Tài liệu này cung cấp một cái nhìn tổng quan về kho lưu trữ dự án "Cây Gia Phả", bao gồm mục đích, công nghệ sử dụng, cấu trúc dự án và các hướng dẫn phát triển.

## 1. Tổng quan Dự án

**Tên Dự án:** Dự án Cây Gia Phả (Family Tree Project)
**Mục đích:** Hệ thống quản lý gia phả chuyên nghiệp cho dòng họ và gia đình, cho phép xây dựng, quản lý và trực quan hóa cây gia phả một cách dễ dàng.

**Các tính năng chính:**
*   Quản lý Dòng họ/Gia đình, Thành viên, Quan hệ.
*   Trực quan hóa Cây Gia Phả (biểu đồ tương tác).
*   Tìm kiếm & Lọc thành viên.
*   Hỗ trợ đa ngôn ngữ (tiếng Việt, tiếng Anh).
*   Các tính năng nâng cao như xuất/nhập GEDCOM, báo cáo thống kê, quản lý tài khoản và phân quyền, tích hợp AI (gợi ý tiểu sử, nhận diện khuôn mặt), cộng tác thời gian thực, thông báo sự kiện, v.v.

## 2. Công nghệ sử dụng (Tech Stack)

*   **Backend:** ASP.NET 8, Clean Architecture, JWT Authentication, MySQL (qua Entity Framework Core).
*   **Frontend:** Vue.js 3, Vuetify 3, Pinia, Vue Router, Vite.
*   **Triển khai:** Docker, Nginx.
*   **CI/CD:** GitHub Actions.

## 3. Bắt đầu nhanh (Getting Started)

### Yêu cầu

*   Docker & Docker Compose (để chạy ứng dụng).
*   .NET 8 SDK (chỉ cần cho phát triển backend).
*   Node.js 20+ (chỉ cần cho phát triển frontend).

### Cài đặt và Chạy

1.  **Clone kho lưu trữ:**
    ```bash
    git clone https://github.com/your-username/family-tree-3.git
    cd family-tree-3
    ```
2.  **Chạy ứng dụng với Docker Compose:**
    Lệnh này sẽ build (nếu cần) và chạy backend, frontend, và database.
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```
3.  **Cấu hình MySQL:**
    Đảm bảo chuỗi kết nối trong `backend/src/Web/appsettings.json` được cấu hình đúng. Sau đó chạy migrations để tạo schema database:
    ```bash
    dotnet ef database update --project backend/src/Infrastructure --startup-project backend/src/Web
    ```
4.  **Truy cập ứng dụng:**
    *   **Frontend:** [http://localhost](http://localhost)
    *   **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 4. Cấu trúc Dự án

*   `backend/`: Chứa mã nguồn ASP.NET Core cho API backend, được tổ chức theo Clean Architecture (Domain, Application, Infrastructure, Web).
*   `frontend/`: Chứa mã nguồn ứng dụng Vue.js frontend.
*   `infra/`: Chứa các tệp cấu hình cho Docker (docker-compose.yml, Dockerfile.backend, Dockerfile.frontend), Nginx và seed data.
*   `docs/`: Chứa toàn bộ tài liệu dự án, được phân loại thành các thư mục con:
    *   `1_product/`: Tài liệu sản phẩm (backlog, epics, yêu cầu, ước tính story, v.v.).
    *   `2_technical/`: Tài liệu kỹ thuật (thiết kế API, hướng dẫn đóng góp, hướng dẫn phát triển, thiết kế hệ thống, v.v.).
    *   `3_process/`: Tài liệu quy trình (thiết lập bảng Kanban, lộ trình, kế hoạch sprint).
    *   `4_testing/`: Các kịch bản kiểm thử.
    *   `5_user/`: Hướng dẫn sử dụng.

## 5. Tổng quan Tài liệu

Thư mục `docs/` chứa các tài liệu quan trọng sau:

*   **Product Backlog (`backlog.md`):** Danh sách chi tiết các User Story.
*   **Epics (`epics.md`):** Phân loại User Story theo các nhóm chức năng lớn.
*   **Requirements (`requirements.md`):** Tài liệu yêu cầu chi tiết, bao gồm chức năng và phi chức năng.
*   **API Design (`api_design.md`):** Mô tả các endpoint API của backend.
*   **System Design (`system_design.md`):** Kiến trúc tổng quan, sơ đồ hệ thống và schema database.
*   **Developer Guide (`developer_guide.md`):** Hướng dẫn cài đặt môi trường, chạy dự án, test, linting, v.v.
*   **Roadmap (`roadmap.md`):** Lộ trình phát triển sản phẩm theo quý.
*   **Sprint Planning (`sprint_planning.md`):** Kế hoạch chi tiết cho các sprint đầu tiên.
*   **Test Cases (`TestCases.md`):** Các kịch bản kiểm thử cho các chức năng chính.

## 6. Hướng dẫn Phát triển

*   **Code Style & Linting:** Sử dụng `dotnet format` cho backend và `eslint` cho frontend.
*   **Testing:** Chạy unit tests và kiểm tra code coverage cho cả backend (`./run-coverage.sh`) và frontend (`npm run test:coverage --prefix frontend`).
*   **Quy trình Pull Request:** Tuân thủ quy tắc đặt tên branch, commit message (Conventional Commits) và checklist review code.
*   **Chiến lược nhánh:** Sử dụng `main`, `develop`, `feature/`, `bugfix/`, `hotfix/`, `docs/`.
*   **Logging & Xử lý lỗi:** Sử dụng Serilog cho logging và middleware xử lý lỗi tập trung.
*   **Quản lý Schema Database:** Sử dụng Entity Framework Core Migrations.
*   **Seed Data:** Có script để populate database với dữ liệu mẫu (`infra/seeds`).

## 7. Ghi chú Gỡ lỗi (Debugging Notes)

Phần này ghi lại các lỗi đã gặp và cách khắc phục để tham khảo trong tương lai.

### a. Lỗi Build Docker do sai Build Context

*   **Vấn đề:** Lệnh `docker-compose -f infra/docker-compose.yml up --build` thất bại với các lỗi như `COPY backend/ ./backend/: not found` hoặc `MSBUILD : error MSB1009: Project file does not exist`.
*   **Nguyên nhân:** Có sự không nhất quán về `build context` giữa môi trường local (`docker-compose.yml`) và CI/CD (`.github/workflows/ci.yml`).
    *   `docker-compose.yml` ban đầu sử dụng `context: ../backend` và `context: ../frontend`, khiến build context là các thư mục con.
    *   `ci.yml` sử dụng `context: .`, khiến build context là thư mục gốc của kho lưu trữ.
    *   Các `Dockerfile` được viết với giả định build context là thư mục gốc.
*   **Giải pháp:**
    1.  **Đồng bộ `docker-compose.yml` với `ci.yml`:** Sửa đổi `infra/docker-compose.yml` để sử dụng `context: .` cho cả hai dịch vụ `backend` và `frontend`.
    2.  **Cập nhật đường dẫn `Dockerfile`:** Đảm bảo đường dẫn `dockerfile` trong `docker-compose.yml` là chính xác so với context mới (ví dụ: `dockerfile: infra/Dockerfile.backend`).
    3.  **Khôi phục `Dockerfile`:** Hoàn nguyên các `Dockerfile` về trạng thái ban đầu, sử dụng các đường dẫn tương đối so với thư mục gốc (ví dụ: `COPY backend/ ./backend/`).

### b. Lỗi Frontend Build do thiếu `sass-embedded`

*   **Vấn đề:** Build frontend thất bại với lỗi `Preprocessor dependency "sass-embedded" not found`.
*   **Nguyên nhân:** `sass-embedded` là một `devDependency` và không được cài đặt trong môi trường build production của Docker.
*   **Giải pháp:** Chuyển `sass-embedded` từ `devDependencies` thành `dependencies` trong tệp `frontend/package.json`.

### c. Lỗi TypeScript `verbatimModuleSyntax`

*   **Vấn đề:** Build frontend thất bại với lỗi `error TS1484: 'TypeName' is a type and must be imported using a type-only import when 'verbatimModuleSyntax' is enabled.`
*   **Nguyên nhân:** Khi `verbatimModuleSyntax` được bật trong `tsconfig.json`, các import chỉ dành cho type phải được đánh dấu rõ ràng.
*   **Giải pháp:** Sửa đổi các câu lệnh import trong các tệp `.vue` bị ảnh hưởng.
    *   **Sai:** `import { MyType } from './my-file';`
    *   **Đúng:** `import type { MyType } from './my-file';`