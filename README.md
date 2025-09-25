# Dự án Cây Gia Phả (Family Tree Project)

Một hệ thống quản lý gia phả chuyên nghiệp cho phép bạn xây dựng, quản lý và trực quan hóa cây gia phả của gia đình một cách dễ dàng.

## ✨ Tính Năng Chính

-   **Quản lý Gia đình/Dòng họ:** Tạo và quản lý thông tin cho nhiều gia đình hoặc dòng họ khác nhau.
-   **Quản lý Thành viên:** Thêm, sửa và xóa thông tin chi tiết cho từng thành viên (tên, ngày sinh/mất, thế hệ, v.v.).
-   **Quản lý Mối quan hệ:** Thiết lập các mối quan hệ (cha/mẹ, vợ/chồng, con) giữa các thành viên.
-   **Trực quan hóa Cây Gia Phả:** Xem cây gia phả dưới dạng một biểu đồ tương tác có khả năng phóng to, di chuyển và lọc.
-   **Tìm kiếm & Lọc:** Dễ dàng tìm kiếm thành viên theo tên, thế hệ và các tiêu chí khác.
-   **Đa ngôn ngữ:** Giao diện hỗ trợ cả tiếng Việt và tiếng Anh.

## 🛠️ Công Nghệ Sử Dụng

-   **Backend:** .NET 8, Clean Architecture, JWT Authentication
-   **Frontend:** Vue.js 3, Vuetify 3, Pinia, Vue Router, Vite
-   **Cơ sở dữ liệu:** MySQL (thông qua Entity Framework Core)
-   **Triển khai:** Docker, Nginx
-   **CI/CD:** GitHub Actions

## 🚀 Bắt Đầu Nhanh

### Yêu Cầu Cần Thiết

-   Docker & Docker Compose
-   .NET 8 SDK (cho phát triển backend)
-   Node.js 20+ (cho phát triển frontend)

### Cài Đặt và Chạy

1.  **Clone repository:**
    ```bash
    git clone https://github.com/hkthao/family-tree-3.git
    cd family-tree-3
    ```

2.  **Chạy ứng dụng với Docker Compose:**
    Lệnh này sẽ build và chạy backend, frontend, và cơ sở dữ liệu.
    ```bash
    docker-compose -f infra/docker-compose.yml up --build
    ```

3.  **Truy cập ứng dụng:**
    -   **Frontend:** [http://localhost](http://localhost)
    -   **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

Để có hướng dẫn chi tiết hơn, bao gồm cách chạy các dịch vụ riêng lẻ để phát triển, vui lòng tham khảo [Hướng dẫn cho Developer](./docs/2_technical/developer_guide.md).

## 📚 Tài Liệu Dự Án

Tất cả tài liệu dự án được đặt trong thư mục [`docs/`](./docs/). Dưới đây là một số tài liệu quan trọng để bạn bắt đầu:

-   [**Hướng dẫn cho Developer**](./docs/2_technical/developer_guide.md): Hướng dẫn toàn diện để thiết lập môi trường phát triển, chạy dự án và hiểu quy trình phát triển.
-   [**Thiết kế Hệ thống**](./docs/2_technical/system_design.md): Tổng quan về kiến trúc hệ thống, các sơ đồ và thiết kế cơ sở dữ liệu.
-   [**Thiết kế API**](./docs/2_technical/api_design.md): Tài liệu chi tiết về các điểm cuối API.

## 🤝 Đóng Góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [Hướng dẫn Đóng góp](./docs/2_technical/contribution.md) của chúng tôi để biết chi tiết về quy tắc ứng xử và quy trình gửi pull request.

## 📄 Giấy Phép

Dự án này được cấp phép theo Giấy phép MIT. Xem tệp [LICENSE](./LICENSE) để biết chi tiết.
