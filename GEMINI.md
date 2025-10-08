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
    *   `engineering/`: Tài liệu kỹ thuật (kiến trúc, hướng dẫn phát triển, API, mô hình dữ liệu, kiểm thử, bảo mật).
    *   `project/`: Tài liệu quản lý dự án (backlog, sprint, test cases, release notes, roadmap, team).

## 5. Tổng quan Tài liệu

Thư mục `docs/` chứa các tài liệu quan trọng sau:

*   [**Tổng quan Tài liệu**](./docs/README.md): Giới thiệu và liên kết đến tất cả các tài liệu con.
*   [**Kiến trúc hệ thống**](./docs/engineering/architecture.md): Mô tả kiến trúc tổng quan, sơ đồ hệ thống và schema database.
*   [**Hướng dẫn Phát triển**](./docs/engineering/development-guide.md): Hướng dẫn cài đặt môi trường, chạy dự án, test, linting, v.v.
*   [**Tham chiếu API**](./docs/engineering/api-reference.md): Mô tả các endpoint API của backend.
*   [**Product Backlog**](./docs/project/backlog.md): Danh sách chi tiết các User Story.
*   [**Kế hoạch Sprint**](./docs/project/sprints.md): Kế hoạch chi tiết cho các sprint.
*   [**Kịch bản Kiểm thử**](./docs/project/test-cases.md): Các kịch bản kiểm thử cho các chức năng chính.
*   [**Ghi chú phát hành**](./docs/project/release-notes.md): Lịch sử các phiên bản và thay đổi.
*   [**Lộ trình Phát triển**](./docs/project/roadmap.md): Lộ trình phát triển sản phẩm theo quý.
*   [**Đội ngũ Phát triển**](./docs/project/team.md): Thông tin về các thành viên trong đội.

## 6. Hướng dẫn Phát triển

*   **Code Style & Linting:** Sử dụng `dotnet format` cho backend và `eslint` cho frontend.
*   **Testing:** Chạy unit tests và kiểm tra code coverage cho cả backend và frontend. Chi tiết tại [Hướng dẫn Kiểm thử](./docs/engineering/testing-guide.md).
*   **Quy trình Pull Request:** Tuân thủ quy tắc đặt tên branch, commit message (Conventional Commits) và checklist review code. Chi tiết tại [Hướng dẫn Đóng góp](./docs/engineering/contribution-guide.md).
*   **Chiến lược nhánh:** Sử dụng `main`, `develop`, `feature/`, `bugfix/`, `hotfix/`, `docs/`.
*   **Logging & Xử lý lỗi:** Sử dụng Serilog cho logging và middleware xử lý lỗi tập trung.
*   **Quản lý Schema Database:** Sử dụng Entity Framework Core Migrations.
*   **Seed Data:** Có script để populate database với dữ liệu mẫu (`infra/seeds`).

## 7. Các thay đổi gần đây

- **Cập nhật tính năng AI Biography:**
  - Backend: Chuyển đổi endpoint lấy tiểu sử AI gần nhất từ trả về chuỗi sang đối tượng DTO đầy đủ, cập nhật API và sử dụng AutoMapper cho việc ánh xạ DTO.
  - Frontend: Cập nhật giao diện người dùng để hiển thị dữ liệu tiểu sử AI đầy đủ, bao gồm tên nhà cung cấp AI và thêm validation cho độ dài prompt.
- **Cải thiện trải nghiệm người dùng:**
  - Thêm tooltips cho tất cả các nút hành động (chỉnh sửa, xóa, thêm mới) trong các danh sách (Thành viên, Gia đình, Sự kiện, Quan hệ).
  - Thêm tooltips cho các nút thu gọn/mở rộng trong các bộ lọc tìm kiếm nâng cao.
- **Dọn dẹp mã nguồn:**
  - Xóa bỏ các import và biến không sử dụng trong các tệp frontend để cải thiện chất lượng mã nguồn và loại bỏ cảnh báo linting.
- **Quản lý Tùy chọn Người dùng:**
  - Backend: Triển khai API riêng biệt để lưu trữ và truy xuất tùy chọn cá nhân của người dùng (chủ đề, ngôn ngữ, cài đặt thông báo qua email/SMS/ứng dụng).
  - Database: Thêm thực thể `UserPreference` và các enum `Theme`, `Language` để lưu trữ các tùy chọn này, đồng thời cập nhật schema database thông qua migration.
  - Cập nhật tài liệu liên quan để phản ánh các thay đổi về API và mô hình dữ liệu.
