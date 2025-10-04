# Dự án Cây Gia Phả (Family Tree Project)

Một hệ thống quản lý gia phả chuyên nghiệp cho phép bạn xây dựng, quản lý và trực quan hóa cây gia phả của gia đình một cách dễ dàng.

## ✨ Tính Năng Chính

-   **Quản lý Gia đình/Dòng họ:** Tạo, xem, chỉnh sửa và quản lý thông tin cho nhiều gia đình hoặc dòng họ khác nhau.
-   **Quản lý Thành viên:** Thêm, sửa, xóa và xem thông tin chi tiết cho từng thành viên (tên, ngày sinh/mất, giới tính, nghề nghiệp, v.v.).
-   **Quản lý Mối quan hệ:** Thiết lập các mối quan hệ (cha/mẹ, vợ/chồng, con) giữa các thành viên, hỗ trợ các mối quan hệ phức tạp.
-   **Quản lý Sự kiện:** Thêm, sửa, xóa và xem các sự kiện quan trọng của gia đình (sinh, kết hôn, mất, họp mặt).
-   **Trực quan hóa Cây Gia Phả:** Xem cây gia phả dưới dạng một biểu đồ tương tác có khả năng phóng to, di chuyển, lọc và nhiều kiểu hiển thị khác nhau.
-   **Tìm kiếm & Lọc:** Dễ dàng tìm kiếm thành viên và dòng họ theo tên, ngày sinh, giới tính và các tiêu chí khác.
-   **Đa ngôn ngữ:** Giao diện hỗ trợ cả tiếng Việt và tiếng Anh.
-   **Xuất/Nhập dữ liệu:** Hỗ trợ xuất/nhập cây gia phả theo các định dạng phổ biến (ví dụ: GEDCOM, PDF).
-   **Báo cáo & Thống kê:** Cung cấp các báo cáo thống kê chi tiết về gia phả.
-   **Tích hợp AI (Kế hoạch)**: Gợi ý tiểu sử, nhận diện khuôn mặt để tự động gắn thẻ.

## 🛠️ Công Nghệ Sử Dụng

-   **Backend:** .NET 8, Clean Architecture, ASP.NET Core, Entity Framework Core, JWT Authentication, MediatR, FluentValidation
-   **Frontend:** Vue.js 3, TypeScript, Vite, Vuetify 3, Pinia, Vue Router, Axios, ESLint, Prettier
-   **Cơ sở dữ liệu:** MySQL
-   **Triển khai:** Docker, Docker Compose, Nginx
-   **CI/CD:** GitHub Actions

## 🚀 Bắt Đầu Nhanh

### Yêu Cầu Cần Thiết

-   **Docker & Docker Compose**: Phiên bản mới nhất.
-   **.NET 8 SDK**: Phiên bản 8.0.x (hoặc mới hơn).
-   **Node.js 20+**: Phiên bản 20.x (hoặc mới hơn).
-   **Công cụ CLI**: `dotnet-ef` để quản lý Entity Framework Core migrations (cài đặt bằng `dotnet tool install --global dotnet-ef`).

### Cài Đặt và Chạy

1.  **Clone repository:**
    ```bash
    git clone https://github.com/hkthao/family-tree-3.git
    cd family-tree-3
    ```

2.  **Chạy ứng dụng với Docker Compose:**
    Lệnh này sẽ build (nếu cần) và chạy backend, frontend, và cơ sở dữ liệu. Đây là cách nhanh nhất để khởi động toàn bộ hệ thống.
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```

3.  **Cấu hình Database (chỉ lần đầu)**:
    Nếu bạn chạy Backend với MySQL (không phải In-Memory Database), bạn cần áp dụng migrations để tạo schema database và seed dữ liệu mẫu.
    ```bash
    dotnet ef database update --project backend/src/Infrastructure --startup-project backend/src/Web
    ```

4.  **Truy cập ứng dụng:**
    -   **Frontend:** [http://localhost](http://localhost) (được phục vụ bởi Nginx)
    -   **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

Để có hướng dẫn chi tiết hơn, bao gồm cách chạy các dịch vụ riêng lẻ để phát triển, vui lòng tham khảo [Hướng dẫn Phát triển](./docs/engineering/development-guide.md).

## 📚 Tài Liệu Dự Án

Tất cả tài liệu dự án được đặt trong thư mục [`docs/`](./docs/README.md). Dưới đây là một số tài liệu quan trọng để bạn bắt đầu:

-   [**Kiến trúc tổng quan**](./docs/engineering/architecture.md)
-   [**Hướng dẫn Phát triển**](./docs/engineering/development-guide.md)
-   [**Hướng dẫn Backend**](./docs/engineering/backend-guide.md)
-   [**Hướng dẫn Frontend**](./docs/engineering/frontend-guide.md)
-   [**Tham chiếu API**](./docs/engineering/api-reference.md)
-   [**Mô hình Dữ liệu**](./docs/engineering/data-model.md)
-   [**Hướng dẫn Kiểm thử & QA**](./docs/engineering/testing-guide.md)
-   [**Hướng dẫn Bảo mật**](./docs/engineering/security-guide.md)
-   [**Product Backlog**](./docs/project/backlog.md)
-   [**Lộ trình Phát triển**](./docs/project/roadmap.md)
-   [**Kế hoạch Sprint**](./docs/project/sprints.md)
-   [**Ghi chú phát hành**](./docs/project/release-notes.md)

## 🤝 Đóng Góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [Hướng dẫn Đóng góp](./docs/engineering/contribution-guide.md) của chúng tôi để biết chi tiết về quy tắc ứng xử và quy trình gửi pull request.

## 📄 Giấy Phép

Dự án này được cấp phép theo Giấy phép MIT. Xem tệp [LICENSE](./LICENSE) để biết chi tiết.