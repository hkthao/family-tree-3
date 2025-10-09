# Kế hoạch Sprint

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Sprint 1](#2-sprint-1)
- [3. Sprint 2](#3-sprint-2)

---

## 1. Giới thiệu

Tài liệu này mô tả kế hoạch cho các sprint phát triển của dự án Cây Gia Phả. Sprint là một khoảng thời gian cố định (thường là 1-4 tuần) trong phương pháp phát triển Agile Scrum, nơi đội ngũ phát triển làm việc để hoàn thành một tập hợp các tính năng hoặc mục tiêu cụ thể. Mỗi sprint có một mục tiêu rõ ràng, các User Story dự kiến sẽ được hoàn thành, và danh sách các thành viên tham gia.

Việc lập kế hoạch sprint giúp:

*   **Tập trung**: Đội ngũ tập trung vào một tập hợp nhỏ các mục tiêu trong một khoảng thời gian ngắn.
*   **Minh bạch**: Mọi người đều biết những gì đang được phát triển và khi nào.
*   **Linh hoạt**: Cho phép điều chỉnh kế hoạch dựa trên phản hồi và thay đổi yêu cầu.

## 2. Sprint 1 (2023-10-20 - 2023-10-26) (updated after refactor)

**Mục tiêu:** Hoàn thành thiết lập dự án, chức năng xác thực cơ bản và xây dựng giao diện ban đầu cho việc hiển thị cây gia phả.

**User Stories đã hoàn thành:**
-   [x] US_018: Đăng nhập hệ thống
-   [x] US_019: Xem chi tiết thành viên
-   [ ] US_XXX: Đăng ký tài khoản mới (Chưa hoàn thành)
-   [ ] US_001: Hiển thị cây gia phả cơ bản (Chưa hoàn thành)

**Mô tả chi tiết:**
*   **Backend**: Thiết lập kiến trúc Clean Architecture, cấu hình Entity Framework Core với MySQL, triển khai cơ chế JWT Bearer Token cho xác thực. Hoàn thành endpoint đăng nhập.
*   **Frontend**: Khởi tạo project Vue 3 với Vite, tích hợp Vuetify 3 cho UI, cấu hình Pinia cho quản lý trạng thái. Xây dựng giao diện đăng nhập cơ bản.

**Thành viên tham gia:**
-   Developer A (Backend)
-   Developer B (Frontend)
-   QA A

## 3. Sprint 2 (2023-10-27 - 2023-11-09) (updated after refactor)

**Mục tiêu:** Hoàn thành chức năng quản lý thành viên cơ bản (thêm, sửa, xóa, xem chi tiết) và cải thiện giao diện cây gia phả.

**User Stories đã hoàn thành:**
-   [x] US_015: Thêm thành viên
-   [x] US_016: Chỉnh sửa thành viên
-   [x] US_017: Tìm kiếm thành viên (Tìm kiếm mở rộng)
-   [x] US_019: Xem chi tiết thành viên
-   [x] US_XXX: Xóa thành viên (Đã hoàn thành như một phần của quản lý thành viên)

**Mô tả chi tiết:**
*   **Backend**: Triển khai các endpoint API và logic nghiệp vụ cho việc thêm, sửa, xóa, xem chi tiết thành viên. Tích hợp CQRS cho các thao tác này.
*   **Frontend**: Xây dựng các form và trang hiển thị thông tin thành viên, tích hợp với API Backend. Cải thiện hiển thị các mối quan hệ trên cây gia phả.

**Thành viên tham gia:**
-   Developer A (Backend)
-   Developer B (Frontend)
-   QA A

## 4. Sprint 3 (2025-10-04 - 2025-10-18) (updated after refactor)

**Mục tiêu:** Cải thiện môi trường phát triển, khắc phục các lỗi cấu hình và đảm bảo tính nhất quán của dữ liệu.

**User Stories đã hoàn thành:**
-   [x] US_020: Tạo dòng họ mới
-   [x] US_021: Xem danh sách dòng họ

**Mô tả chi tiết:**
*   **Backend**: 
    *   Cải thiện cấu hình Dependency Injection, cập nhật phiên bản các gói NuGet liên quan đến Entity Framework Core và Pomelo.MySql để tương thích với .NET 8.
    *   Khắc phục lỗi NSwag khi khởi động, đảm bảo quá trình tạo tài liệu API diễn ra suôn sẻ.
    *   Điều chỉnh chuỗi kết nối MySQL cho môi trường phát triển cục bộ và triển khai cơ chế seeding dữ liệu mẫu tự động khi khởi động.
    *   Tạo và áp dụng migration ban đầu cho cơ sở dữ liệu MySQL.
*   **Frontend**: 
    *   Cập nhật định nghĩa kiểu dữ liệu `Family` và `MemberFilter` để giải quyết lỗi kiểm tra kiểu.
    *   Cấu hình Vite proxy để kết nối chính xác với Backend đang chạy cục bộ, khắc phục lỗi CORS và `ECONNREFUSED`.

**Thành viên tham gia:**
-   Gemini (AI Assistant)
-   Developer A (Backend)
-   Developer B (Frontend)

## 5. Sprint 4 (2025-10-19 - 2025-11-02)

**Mục tiêu:** Triển khai chức năng quản lý mối quan hệ (CRUD) đầy đủ cho Frontend và Backend.

**User Stories dự kiến hoàn thành:**
-   [x] US_022: Thêm mối quan hệ
-   [x] US_023: Chỉnh sửa mối quan hệ
-   [x] US_024: Xóa mối quan hệ
-   [x] US_025: Xem danh sách mối quan hệ

**Mô tả chi tiết:**
*   **Backend**: Triển khai các endpoint API, logic nghiệp vụ và các Specification cho việc thêm, sửa, xóa, xem chi tiết và tìm kiếm mối quan hệ. Đảm bảo sử dụng `Result Pattern` nhất quán.
*   **Frontend**: Xây dựng các components (list, form, search) và views cho quản lý mối quan hệ. Tích hợp Pinia store và API service. Cập nhật router và menu điều hướng.

**Thành viên tham gia:**
-   Gemini (AI Assistant)
-   Developer A (Backend)
-   Developer B (Frontend)

## 6. Sprint 5 (2025-10-09 - 2025-10-23)

**Mục tiêu:** Cải thiện kiến trúc backend, triển khai tính năng tải lên tệp an toàn và khắc phục các lỗi liên quan đến xác thực và hiển thị thông tin người dùng.

**Tính năng mới:**
-   [x] US_010: Tải lên và quản lý tệp đính kèm

**Cải tiến & Sửa lỗi:**
*   **Refactor Kiến trúc Backend**: 
    -   Tạo project `CompositionRoot` để thiết lập Dependency Injection tập trung, loại bỏ sự phụ thuộc trực tiếp của Web Layer vào Infrastructure Layer.
    -   Loại bỏ sự phụ thuộc của Domain Layer vào `MediatR`.
    -   Tái thêm các dependency `Microsoft.EntityFrameworkCore` và `Ardalis.Specification.EntityFrameworkCore` vào Application Layer (giải pháp thực dụng).
*   **Tách biệt UserProfile khỏi Auth0**: 
    -   Đổi tên `Auth0UserId` thành `ExternalId` trong entity `UserProfile` và DTO ở Backend.
    -   Cập nhật các query, handler và controller liên quan để sử dụng `ExternalId` và endpoint `byExternalId`.
    -   Cập nhật các interface `User` và `UserProfile`, các service (`auth0Service`, `userProfileService`), và các store (`auth.store`, `userProfile.store`) ở Frontend để sử dụng `externalId` nhằm tách biệt khỏi nhà cung cấp xác thực cụ thể.
*   **Thông báo Snackbar**: 
    -   Triển khai thông báo snackbar cho các thao tác lưu tùy chọn người dùng thành công/thất bại trong `PreferencesSettings.vue`.
    -   Điều chỉnh vị trí snackbar hiển thị ở giữa dưới cùng.
*   **Xử lý Callback Auth0**: Khắc phục lỗi reload liên tục sau khi xử lý callback từ Auth0 bằng cách thay thế URL.
*   **Tải thông tin User Profile**: Đảm bảo `ProfileSettings.vue` tải và hiển thị thông tin hồ sơ người dùng chính xác khi component được mount.

**Thành viên tham gia:**
-   Gemini (AI Assistant)
-   Developer A (Backend)
-   Developer B (Frontend)