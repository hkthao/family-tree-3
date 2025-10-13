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

## 3. Phiên bản 0.3.0 (2025-10-09)

### Tính năng mới

-   **API Tải lên tệp an toàn**: Triển khai API tải lên tệp với các nhà cung cấp lưu trữ có thể chuyển đổi (Local, Cloudinary, AWS S3), tuân thủ Clean Architecture và các yêu cầu bảo mật.

### Cải tiến & Sửa lỗi

-   **Refactor Kiến trúc Backend**: 
    -   Tạo project `CompositionRoot` để thiết lập Dependency Injection tập trung, loại bỏ sự phụ thuộc trực tiếp của Web Layer vào Infrastructure Layer.
    -   Loại bỏ sự phụ thuộc của Domain Layer vào `MediatR`.
    -   Tái thêm các dependency `Microsoft.EntityFrameworkCore` và `Ardalis.Specification.EntityFrameworkCore` vào Application Layer (giải pháp thực dụng).
-   **Tách biệt UserProfile khỏi nhà cung cấp xác thực**: 
    -   Đổi tên `Auth0UserId` thành `ExternalId` trong entity `UserProfile` và DTO ở Backend.
    -   Cập nhật các query, handler và controller liên quan để sử dụng `ExternalId` và endpoint `byExternalId`.
    -   Cập nhật các interface `User` và `UserProfile`, các service (`auth0Service`, `userProfileService`), và các store (`auth.store`, `userProfile.store`) ở Frontend để sử dụng `externalId` nhằm tách biệt khỏi nhà cung cấp xác thực cụ thể (ví dụ: Auth0).
-   **Thông báo Snackbar**: 
    -   Triển khai thông báo snackbar cho các thao tác lưu tùy chọn người dùng thành công/thất bại trong `PreferencesSettings.vue`.
    -   Điều chỉnh vị trí snackbar hiển thị ở giữa dưới cùng.
-   **Xử lý Callback Auth0**: Khắc phục lỗi reload liên tục sau khi xử lý callback từ Auth0 bằng cách thay thế URL.
-   **Tải thông tin User Profile**: Đảm bảo `ProfileSettings.vue` tải và hiển thị thông tin hồ sơ người dùng chính xác khi component được mount.
-   **Dọn dẹp cấu hình**: Xóa bỏ các comment trong `appsettings.json`.

### Các vấn đề đã biết

-   Chức năng tìm kiếm vẫn chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.

---

## 4. Phiên bản 0.2.0 (2025-10-04)

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

---

## 5. Phiên bản 0.4.0 (2025-10-13)

### Tính năng mới

-   **Quản lý Tùy chọn Người dùng**: Triển khai API riêng biệt để lưu trữ và truy xuất tùy chọn cá nhân của người dùng (chủ đề, ngôn ngữ, cài đặt thông báo qua email/SMS/ứng dụng). Thêm thực thể `UserPreference` và các enum `Theme`, `Language` để lưu trữ các tùy chọn này, đồng thời cập nhật schema database thông qua migration.
-   **Cập nhật tính năng AI Biography**: Chuyển đổi endpoint lấy tiểu sử AI gần nhất từ trả về chuỗi sang đối tượng DTO đầy đủ, cập nhật API và sử dụng AutoMapper cho việc ánh xạ DTO. Cập nhật giao diện người dùng để hiển thị dữ liệu tiểu sử AI đầy đủ, bao gồm tên nhà cung cấp AI và thêm validation cho độ dài prompt.
-   **Xử lý dữ liệu và Chia Chunk**: Triển khai module xử lý và chia nhỏ nội dung từ các tệp PDF/TXT thành các `TextChunk` với đầy đủ metadata (fileId, familyId, category, createdBy) để chuẩn bị cho việc tạo embeddings và tích hợp chatbot.

### Cải tiến & Sửa lỗi

-   **Cải thiện trải nghiệm người dùng**: Thêm tooltips cho tất cả các nút hành động (chỉnh sửa, xóa, thêm mới) trong các danh sách (Thành viên, Gia đình, Sự kiện, Quan hệ). Thêm tooltips cho các nút thu gọn/mở rộng trong các bộ lọc tìm kiếm nâng cao.
-   **Dọn dẹp mã nguồn**: Xóa bỏ các import và biến không sử dụng trong các tệp frontend để cải thiện chất lượng mã nguồn và loại bỏ cảnh báo linting.
-   **Cập nhật API Hồ sơ Người dùng**: Thêm endpoint backend mới `GET /api/UserProfiles/me` để lấy hồ sơ của người dùng hiện tại một cách an toàn.
-   **Trường Avatar cho Hồ sơ Người dùng**: Bổ sung trường `Avatar` vào thực thể `UserProfile` và cập nhật các chức năng liên quan ở cả backend và frontend.
-   **Xử lý kết quả nhất quán**: Triển khai `Result` wrapper cho `GetCurrentUserProfileQueryHandler` để đảm bảo xử lý kết quả nhất quán.

### Các vấn đề đã biết

-   Chức năng tìm kiếm vẫn chưa được tối ưu.
-   Cây gia phả có thể hiển thị chậm với số lượng lớn thành viên.