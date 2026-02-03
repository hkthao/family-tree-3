# Ngăn xếp Công nghệ (Tech Stack)

Tài liệu này liệt kê và mô tả các công nghệ chính được sử dụng trong dự án "Cây Gia Phả". Việc hiểu rõ ngăn xếp công nghệ giúp các thành viên trong nhóm và các bên liên quan nắm bắt được nền tảng kỹ thuật của dự án.

## 1. Tổng quan

Dự án được xây dựng trên nền tảng kiến trúc microservices và monorepo, sử dụng sự kết hợp của các công nghệ mạnh mẽ và hiện đại cho cả frontend, backend và các dịch vụ phụ trợ.

## 2. Công nghệ Backend

*   **Ngôn ngữ:** C#
*   **Framework:** ASP.NET 8
*   **Kiến trúc:** Clean Architecture, DDD (Domain-Driven Design), CQRS (Command Query Responsibility Segregation)
*   **Thư viện/Công cụ:**
    *   **Entity Framework Core:** ORM (Object-Relational Mapper) để tương tác với cơ sở dữ liệu.
    *   **MediatR:** Thư viện triển khai CQRS và Mediator pattern.
    *   **FluentValidation:** Thư viện để định nghĩa và thực thi các quy tắc validation.
    *   **JWT Authentication:** JSON Web Token để xác thực người dùng và bảo mật API.
    *   **Novu:** (Nếu được sử dụng trong Notification Service) Có thể tích hợp với backend cho hệ thống thông báo.
*   **Cơ sở dữ liệu:** MySQL

## 3. Công nghệ Frontend (Admin)

*   **Ngôn ngữ:** TypeScript
*   **Framework:** Vue.js 3
*   **Build Tool:** Vite
*   **UI Framework:** Vuetify 3 (Material Design components)
*   **Quản lý trạng thái:** Pinia
*   **Routing:** Vue Router
*   **HTTP Client:** Axios (custom `apiClient`)
*   **Validation:** Vuelidate (hoặc các thư viện tương tự để validation form)
*   **Quốc tế hóa (i18n):** Vue I18n
*   **Code Quality:** ESLint, Prettier

## 4. Công nghệ Dịch vụ (Microservices)

Dự án sử dụng nhiều microservices chuyên biệt được triển khai trong thư mục `services/`:

*   **`face-service`:**
    *   **Ngôn ngữ:** Python
    *   **Chức năng:** Xử lý các tác vụ liên quan đến khuôn mặt (nhận diện, so sánh, v.v.).
*   **`graphviz-pdf-converter`:**
    *   **Ngôn ngữ:** Python
    *   **Chức năng:** Chuyển đổi định dạng Graphviz DOT sang PDF.
*   **`knowledge-search-service`:**
    *   **Ngôn ngữ:** Python
    *   **Chức năng:** Cung cấp khả năng tìm kiếm kiến thức trong hệ thống.
*   **`llm-gateway-service`:**
    *   **Ngôn ngữ:** Python
    *   **Chức năng:** Cổng truy cập và quản lý các mô hình ngôn ngữ lớn (LLM).
*   **`notification-service`:**
    *   **Ngôn ngữ:** Node.js
    *   **Chức năng:** Quản lý và gửi các loại thông báo khác nhau.
*   **`storage-service`:**
    *   **Ngôn ngữ:** Node.js
    *   **Chức năng:** Quản lý lưu trữ tệp tin.


## 5. Hạ tầng và DevOps

*   **Containerization:** Docker
*   **Orchestration:** Docker Compose (dùng cho môi trường phát triển và triển khai đơn giản)
*   **Web Server/Reverse Proxy:** Nginx
*   **CI/CD:** GitHub Actions (tự động hóa build, test, lint, và triển khai)

## 6. Thư viện Trực quan hóa

*   **D3.js:** Thư viện JavaScript mạnh mẽ cho việc thao tác tài liệu dựa trên dữ liệu, thường dùng để tạo các biểu đồ phức tạp hoặc trực quan hóa dữ liệu tùy chỉnh.
*   **ApexCharts:** Thư viện biểu đồ hiện đại, tương tác.
*   **Family-chart (f3):** Thư viện chuyên biệt để vẽ cây gia phả.

## 7. Các Công cụ Khác

*   **.NET SDK:** Môi trường phát triển cho các ứng dụng .NET.
*   **Node.js:** Môi trường runtime cho JavaScript (frontend và một số microservices).
*   **Java 17+:** (Cho phát triển mobile, nếu có ứng dụng di động riêng).
*   **ESLint, Prettier:** Công cụ kiểm tra và định dạng mã nguồn.

---
**Lưu ý:** Danh sách này sẽ được cập nhật định kỳ để phản ánh chính xác các công nghệ đang được sử dụng trong dự án.
