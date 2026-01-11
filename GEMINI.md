# Tóm tắt Kho lưu trữ Dự án Cây Gia Phả

Tài liệu này cung cấp một cái nhìn tổng quan về kho lưu trữ dự án "Cây Gia Phả", bao gồm mục đích, công nghệ sử dụng, cấu trúc dự án và các hướng dẫn phát triển.

## 1. Tổng quan Dự án

**Tên Dự án:** Dự án Dòng Họ Việt (Family Tree Project)
**Mục đích:** Hệ thống quản lý gia phả chuyên nghiệp cho dòng họ và gia đình, cho phép xây dựng, quản lý và trực quan hóa cây gia phả một cách dễ dàng.

**Các tính năng chính:**
*   Quản lý Dòng họ/Gia đình, Thành viên, Quan hệ.
*   Trực quan hóa Cây Gia Phả (biểu đồ tương tác).
*   Tìm kiếm & Lọc thành viên.
*   Hỗ trợ đa ngôn ngữ (tiếng Việt, tiếng Anh).
*   Các tính năng nâng cao như xuất/nhập GEDCOM, báo cáo thống kê, quản lý tài khoản và phân quyền, tích hợp AI (gợi ý tiểu sử, nhận diện khuôn mặt, xử lý ngôn ngữ tự nhiên), cộng tác thời gian thực, thông báo sự kiện, v.v.

## 2. Công nghệ sử dụng (Tech Stack)

*   **Backend:** ASP.NET 8, Clean Architecture, JWT Authentication, MySQL (qua Entity Framework Core).
*   **Frontend (Admin):** Vue.js 3, TypeScript, Vuetify 3, Pinia, Vue Router, Vite, Axios (custom `apiClient`), Vuelidate, Vue I18n.
*   **Triển khai:** Docker, Nginx.
*   **CI/CD:** GitHub Actions.
*   **Trực quan hóa:** D3.js, ApexCharts, Family-chart (f3).

## 3. Bắt đầu nhanh (Getting Started)

### Yêu cầu

*   Docker & Docker Compose (để chạy ứng dụng).
*   .NET 8 SDK (chỉ cần cho phát triển backend).
*   Node.js 20+ (chỉ cần cho phát triển frontend).
*   Java 17+ (chỉ cần cho phát triển mobile).

### Cài đặt và Chạy

1.  **Clone kho lưu trữ:**
    ```bash
    git clone https://github.com/your-username/family-tree-3.git
    cd family-tree-3
    ```
2.  **Chạy ứng dụng với Docker Compose:**
    Lệnh này sẽ build (nếu cần) và chạy backend, admin, face-service và database.
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```
3.  **Cấu hình MySQL:**
    Đảm bảo chuỗi kết nối trong `apps/backend/src/Web/appsettings.json` được cấu hình đúng. Sau đó chạy migrations để tạo schema database:
    ```bash
    dotnet ef database update --project apps/backend/src/Infrastructure --startup-project apps/backend/src/Web
    ```
4.  **Truy cập ứng dụng:**
    *   **Admin Frontend:** [http://localhost:8081](http://localhost:8081)
    *   **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)
5.  **Chạy ứng dụng di động (Tùy chọn):**
    ```bash
    cd apps/mobile/family_tree_rn
    npm install
    npm run android # hoặc npm run ios
    ```

## 4. Cấu trúc Dự án

Dự án được tổ chức theo cấu trúc monorepo, bao gồm các thư mục chính sau:

*   `apps/`: Chứa các ứng dụng chính có thể chạy độc lập.
    *   `apps/backend/`: Mã nguồn ASP.NET Core API, tổ chức theo Clean Architecture (Domain, Application, Infrastructure, Web).
    *   `apps/admin/`: Mã nguồn ứng dụng Vue.js frontend cho giao diện quản trị.
*   `services/`: Chứa các dịch vụ phụ trợ.
    *   `services/face-service/`: Dịch vụ xử lý khuôn mặt bằng Python.
    *   `services/puppeteer-service/`: Dịch vụ chuyển đổi HTML/CSS sang PDF bằng Node.js và Puppeteer.
*   `packages/`: (Tùy chọn) Chứa các gói mã nguồn được chia sẻ giữa các ứng dụng.
    *   `packages/shared-types/`: (Tùy chọn) Định nghĩa các kiểu dữ liệu (TypeScript types/interfaces/DTOs) được chia sẻ giữa backend và các frontend.
    *   `packages/ui-components/`: (Tùy chọn) Nơi chứa các thành phần UI dùng chung cho các ứng dụng frontend.
*   `infra/`: Chứa các tệp cấu hình cho Docker (docker-compose.yml, Dockerfile.*), Nginx và seed data.
*   `docs/`: Chứa toàn bộ tài liệu dự án, được phân loại thành các thư mục con:
    *   `engineering/`: Tài liệu kỹ thuật (kiến trúc, hướng dẫn phát triển, API, mô hình dữ liệu, kiểm thử, bảo mật).
    *   `project/`: Tài liệu quản lý dự án (backlog, sprint, test cases, release notes, roadmap, team).
*   `tests/`: Chứa các bài kiểm thử tổng thể hoặc các bài kiểm thử không thuộc về một ứng dụng cụ thể.

## 8. Frontend Conventions

### 8.1. Cấu trúc Service

*   Mỗi service nên có một thư mục riêng trong `apps/admin/src/services/` (ví dụ: `apps/admin/src/services/family/`).
*   Trong thư mục service, sẽ có các tệp sau:
    *   `[tên_service].service.interface.ts`: Định nghĩa interface cho service (ví dụ: `IFamilyService`).
    *   `api.[tên_service].service.ts`: Triển khai service sử dụng API thật (ví dụ: `ApiFamilyService`).
*   Tất cả các service API nên nhận `ApiClientMethods` làm dependency trong constructor.
*   Các phương thức service nên trả về kiểu `Result<T, ApiError>` để xử lý lỗi nhất quán.

### 8.3. Import Paths

*   Khi import các component, service, store hoặc type, luôn sử dụng alias `@/` (ví dụ: `@/stores/family.store`).
*   Khi import một component cụ thể từ một thư mục, hãy chỉ định rõ tệp `.vue` (ví dụ: `import NaturalLanguageInputModal from '@/components/NaturalLanguageInput/NaturalLanguageInputModal.vue';`). Tránh import thư mục trực tiếp (ví dụ: `import { NaturalLanguageInputModal } from '@/components/NaturalLanguageInput';`).