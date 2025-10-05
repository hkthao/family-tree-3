# Ghi chú phát hành (Release Notes)

## Mục lục

- [1. Phiên bản 1.0.0 (2023-10-26)](#1-phiên-bản-100-2023-10-26)
- [2. Phiên bản 0.1.0 (2023-10-20)](#2-phiên-bản-010-2023-10-20)

---

## 1. Phiên bản 1.0.0 (2023-10-26)

### Tính năng mới

-   **Quản lý gia đình**: Người dùng có thể tạo, xem, và quản lý thông tin các gia đình.
-   **Quản lý thành viên**: Hỗ trợ thêm, sửa, và xóa thành viên trong một gia đình.

### Các vấn đề đã biết

-   Chức năng tìm kiếm chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.

---

## 2. Phiên bản 0.1.0 (2023-10-20)

-   Khởi tạo dự án.
-   Thiết lập kiến trúc Clean Architecture cho Backend.
-   Thiết lập project Vue 3 với Vuetify cho Frontend.

---

## 3. Phiên bản 0.2.0 (2025-10-04)

### Cải tiến & Sửa lỗi

-   **Cải thiện thiết lập Backend**: 
    -   Đã thêm lệnh gọi `AddApplicationServices` để đăng ký dịch vụ ứng dụng.
    -   Cấu hình sử dụng cơ sở dữ liệu trong bộ nhớ (in-memory database) cho môi trường phát triển và NSwag.
    -   Cập nhật phiên bản các gói `Pomelo.EntityFrameworkCore.MySql` và `Microsoft.EntityFrameworkCore` để tương thích với .NET 8.
    -   Khắc phục lỗi `NU1008` liên quan đến quản lý phiên bản gói tập trung.
    -   Cập nhật chính sách CORS để cho phép kết nối từ Frontend và Backend.
    -   Điều chỉnh chuỗi kết nối MySQL cho môi trường phát triển cục bộ (`localhost`).
    -   Khắc phục lỗi NSwag khi khởi động do vấn đề với cơ sở dữ liệu quan hệ.
-   **Quản lý Database**: 
    -   Đã tạo và áp dụng migration ban đầu (`InitialCreate`) cho cơ sở dữ liệu MySQL.
    -   Thêm dữ liệu mẫu về gia đình, thành viên và sự kiện vào `ApplicationDbContextInitialiser` để tự động seed khi khởi động ở chế độ phát triển.
-   **Cập nhật Frontend**: 
    -   Cập nhật định nghĩa kiểu dữ liệu `Family` và `MemberFilter` để giải quyết lỗi kiểm tra kiểu.
    -   Cấu hình proxy Vite để kết nối chính xác với Backend đang chạy cục bộ.

### Các vấn đề đã biết

-   Chức năng tìm kiếm vẫn chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.