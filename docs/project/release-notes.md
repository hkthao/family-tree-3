# Ghi chú phát hành (Release Notes)

## Mục lục

- [1. Phiên bản 1.1.0 (2025-10-16)](#1-phiên-bản-110-2025-10-16)
- [2. Phiên bản 1.0.0 (2023-10-26)](#2-phiên-bản-100-2023-10-26)
- [3. Phiên bản 0.1.0 (2023-10-20)](#3-phiên-bản-010-2023-10-20)

---

## 1. Phiên bản 1.1.1 (2025-10-17)

### Cải tiến & Sửa lỗi

-   Cấu hình CORS cho Backend sử dụng biến môi trường `CORS_ORIGINS` từ `src/backend/.env`.
-   Tách biệt các tệp `.env` cho Backend (`src/backend/.env`) và Frontend (`src/frontend/.env`).
-   Khắc phục lỗi "Duplicate entry" khi lưu `UserPreferences`.
-   Cập nhật giao diện `UserPreference` của Frontend để khớp với Backend.
-   Cấu hình AI chat và embedding service để sử dụng `host.docker.internal` khi chạy trong Docker Compose.
-   Cập nhật tài liệu hướng dẫn phát triển, API, kiến trúc, và hướng dẫn backend để phản ánh các thay đổi cấu hình và tính năng mới.

---

## 2. Phiên bản 1.1.0 (2025-10-16)

### Tính năng mới

-   **Quản lý Xác thực & Hồ sơ Người dùng**:
    -   Đăng nhập hệ thống an toàn với JWT Bearer Token.
    -   Refactor `auth0UserId` thành `externalId` để tách biệt khỏi nhà cung cấp xác thực cụ thể.
    -   Quản lý Hồ sơ Người dùng tập trung trong `userProfileStore` (Frontend).
    -   API mới `GET /api/UserProfiles/me` để lấy hồ sơ của người dùng hiện tại.
    -   Hỗ trợ trường `Avatar` cho Hồ sơ Người dùng.
    -   Xử lý kết quả nhất quán với `Result` wrapper cho `GetCurrentUserProfileQueryHandler`.
-   **Quản lý Dòng họ & Thành viên**:
    -   Tạo, xem danh sách, xem chi tiết, chỉnh sửa và xóa dòng họ.
    -   Tạo, xem danh sách, xem chi tiết, chỉnh sửa và xóa thành viên.
    -   Tìm kiếm thành viên với các tùy chọn mở rộng.
    -   Tự động cập nhật thống kê `TotalMembers` và `TotalGenerations` cho gia đình.
-   **Quản lý Quan hệ**:
    -   Thêm, chỉnh sửa, xóa và xem danh sách các mối quan hệ giữa các thành viên.
    -   Hỗ trợ quản lý mối quan hệ phức tạp (ví dụ: cha dượng/mẹ kế, con nuôi).
-   **Quản lý Sự kiện**:
    -   Tạo, chỉnh sửa, xóa và xem danh sách các sự kiện.
    -   API để lấy danh sách các sự kiện sắp tới.
-   **Quản lý Tùy chọn Người dùng**:
    -   Lưu trữ và truy xuất tùy chọn cá nhân của người dùng (chủ đề, ngôn ngữ, cài đặt thông báo).
    -   Thêm thực thể `UserPreference` và các enum `Theme`, `Language`.
-   **Module AI & Dữ liệu**:
    -   **AI Biography**: Sinh tiểu sử cho thành viên bằng AI, hiển thị thông tin đầy đủ hơn và có validation cho prompt.
    -   **Xử lý dữ liệu và Chia Chunk**: Tải lên các tệp tài liệu (PDF/TXT) và hệ thống tự động xử lý, chia nhỏ nội dung thành các `TextChunk` với đầy đủ metadata.
    -   **Tải lên và quản lý tệp đính kèm**: Tải lên và quản lý các tệp (hình ảnh, tài liệu) liên quan đến các thành viên hoặc gia đình.
    -   **Vector Database**: Triển khai module Vector Database với các nhà cung cấp như Pinecone, Qdrant, và In-Memory.
    -   **Chatbot**: API để tương tác với chatbot AI.
    -   **Natural Language Input**: API để tạo dữ liệu sự kiện từ mô tả ngôn ngữ tự nhiên.
-   **Cải thiện trải nghiệm người dùng**:
    -   Thêm tooltips cho các nút hành động và nút thu gọn/mở rộng.
    -   Dọn dẹp mã nguồn Frontend (xóa import và biến không sử dụng).
-   **Cải tiến Kiến trúc & Phát triển**:
    -   Refactor kiến trúc Backend với `CompositionRoot` và tách biệt `UserProfile` khỏi nhà cung cấp xác thực.
    -   Cải thiện thiết lập Backend và Frontend cho môi trường phát triển.
    -   Quản lý database với Entity Framework Core Migrations và Seeding dữ liệu mẫu.
    -   Sử dụng Dịch vụ Phân quyền (`IAuthorizationService`) tập trung.
    -   Sử dụng AutoMapper cho DTO Mapping.
    -   Tích hợp Logging với Serilog.

### Cải tiến & Sửa lỗi

-   Cập nhật định nghĩa kiểu dữ liệu `Family` và `MemberFilter` ở Frontend.
-   Cấu hình proxy Vite để kết nối chính xác với Backend.
-   Khắc phục lỗi reload liên tục sau khi xử lý callback từ Auth0.
-   Điều chỉnh vị trí snackbar hiển thị ở giữa dưới cùng.

### Các vấn đề đã biết

-   Chức năng tìm kiếm vẫn chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.

---

## 2. Phiên bản 1.0.0 (2023-10-26)

### Tính năng mới

-   **Quản lý gia đình**: Người dùng có thể tạo, xem, và quản lý thông tin các gia đình.
-   **Quản lý thành viên**: Hỗ trợ thêm, sửa, và xóa thành viên trong một gia đình.

### Các vấn đề đã biết

-   Chức năng tìm kiếm chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.

---

## 3. Phiên bản 0.1.0 (2023-10-20)

-   Khởi tạo dự án.
-   Thiết lập kiến trúc Clean Architecture cho Backend.
-   Thiết lập project Vue 3 với Vuetify cho Frontend.
