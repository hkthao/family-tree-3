# Hướng dẫn Phát triển Tổng thể

Tài liệu này cung cấp cái nhìn tổng quan về quy trình phát triển cho dự án Dòng Họ Việt, bao gồm thiết lập môi trường, cấu trúc dự án và các quy trình chung. Để biết hướng dẫn chi tiết cho từng phần, vui lòng tham khảo các tài liệu chuyên biệt dưới đây.

## 1. Tổng quan Dự án

Dự án Dòng Họ Việt là một ứng dụng quản lý gia phả được xây dựng theo kiến trúc microservices và Clean Architecture. Nó bao gồm các thành phần chính:

*   **Backend API**: Cung cấp các dịch vụ dữ liệu và logic nghiệp vụ.
*   **Frontend (Admin)**: Giao diện người dùng cho quản trị viên.
*   **Microservices**: Các dịch vụ hỗ trợ chuyên biệt (ví dụ: xử lý khuôn mặt, thông báo, OCR, giọng nói).

## 2. Thiết lập Môi trường Phát triển

Để bắt đầu phát triển, bạn cần cài đặt các công cụ sau:

*   **Docker và Docker Compose**: Để chạy toàn bộ hệ thống (backend, frontend, database, microservices) một cách dễ dàng.
*   **.NET 8 SDK**: Cần thiết cho việc phát triển và xây dựng backend.
*   **Node.js 20+ và npm**: Cần thiết cho việc phát triển và xây dựng frontend.

Để biết hướng dẫn chi tiết về cách cài đặt và chạy dự án, vui lòng tham khảo mục [Cài đặt và Chạy](/#3-cài-đặt-và-chạy) trong `README.md` chính của dự án.

## 3. Cấu trúc Dự án

Dự án được tổ chức theo cấu trúc monorepo:

*   `apps/`: Chứa các ứng dụng chính (backend, admin).
*   `services/`: Chứa các microservices phụ trợ.
*   `infra/`: Chứa các tệp cấu hình cho Docker và Nginx.
*   `docs/`: Chứa toàn bộ tài liệu dự án.

Để biết cấu trúc thư mục chi tiết, tham khảo mục [Cấu trúc Thư mục](/#6-cấu-trúc-thư-mục-project-structure) trong `README.md`.

## 4. Quy trình Phát triển

Khi phát triển tính năng mới hoặc sửa lỗi, hãy tuân thủ quy trình chung sau:

1.  **Tạo nhánh mới:** Luôn làm việc trên một nhánh tính năng mới từ nhánh `develop`.
2.  **Phát triển:** Thực hiện thay đổi ở các phần liên quan (backend, frontend, microservices).
3.  **Kiểm thử:** Viết và chạy các unit/integration tests phù hợp.
4.  **Kiểm tra Code Style/Linting:** Đảm bảo mã nguồn tuân thủ các quy tắc định dạng và linting.
5.  **Tạo Pull Request:** Gửi Pull Request lên nhánh `develop` và yêu cầu review.

## 5. Tài liệu Chuyên biệt

Để biết hướng dẫn chi tiết hơn cho từng thành phần của dự án, vui lòng tham khảo các tài liệu sau:

*   [**Hướng dẫn Phát triển Backend**](./backend-guide.md)
*   [**Hướng dẫn Phát triển Frontend (Admin)**](./frontend-guide.md)
*   [**Mô hình Dữ liệu và Schema Database**](./data-model.md)
*   [**Kiến trúc Hệ thống**](./architecture.md)
*   [**Tham chiếu API**](./api-reference.md)
