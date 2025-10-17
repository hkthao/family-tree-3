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

*   `src/backend/`: Chứa mã nguồn ASP.NET Core cho API backend, được tổ chức theo Clean Architecture (Domain, Application, Infrastructure, Web).
*   `src/frontend/`: Chứa mã nguồn ứng dụng Vue.js frontend.
*   `src/infra/`: Chứa các tệp cấu hình cho Docker (docker-compose.yml, Dockerfile.backend, Dockerfile.frontend), Nginx và seed data.
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
*   **Seed Data:** Có script để populate database với dữ liệu mẫu (`src/infra/seeds`).

## 8. Frontend Conventions

### 8.1. Cấu trúc Service

*   Mỗi service nên có một thư mục riêng trong `src/frontend/src/services/` (ví dụ: `src/frontend/src/services/family/`).
*   Trong thư mục service, sẽ có các tệp sau:
    *   `[tên_service].service.interface.ts`: Định nghĩa interface cho service (ví dụ: `IFamilyService`).
    *   `api.[tên_service].service.ts`: Triển khai service sử dụng API thật (ví dụ: `ApiFamilyService`).
    *   `mock.[tên_service].service.ts`: Triển khai service sử dụng dữ liệu giả (mock data) cho mục đích phát triển/kiểm thử (ví dụ: `MockFamilyService`).
*   Tất cả các service API nên nhận `ApiClientMethods` làm dependency trong constructor.
*   Các phương thức service nên trả về kiểu `Result<T, ApiError>` để xử lý lỗi nhất quán.

### 8.2. Cấu trúc Store (Pinia)

*   Các store nên được định nghĩa theo kiểu Options API của Pinia (sử dụng `state`, `getters`, `actions` làm thuộc tính của đối tượng truyền vào `defineStore`).
*   Các service nên được truy cập thông qua `this.services.[tên_service]` (ví dụ: `this.services.family.loadItems()`). Điều này được thực hiện thông qua `src/frontend/src/plugins/services.plugin.ts`.
*   Thông báo lỗi nên được dịch hóa bằng `i18n.global.t()` (ví dụ: `i18n.global.t('family.errors.load')`).
*   Các hành động (actions) trong store nên cập nhật trạng thái `loading` và `error` một cách nhất quán.

### 8.3. Import Paths

*   Khi import các component, service, store hoặc type, luôn sử dụng alias `@/` (ví dụ: `@/stores/family.store`).
*   Khi import một component cụ thể từ một thư mục, hãy chỉ định rõ tệp `.vue` (ví dụ: `import NaturalLanguageInputModal from '@/components/NaturalLanguageInput/NaturalLanguageInputModal.vue';`). Tránh import thư mục trực tiếp (ví dụ: `import { NaturalLanguageInputModal } from '@/components/NaturalLanguageInput';`).