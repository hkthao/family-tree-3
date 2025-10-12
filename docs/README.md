# Tài Liệu Dự Án Cây Gia Phả

Chào mừng bạn đến với tài liệu của dự án Cây Gia Phả. Tài liệu này cung cấp thông tin toàn diện về kiến trúc, hướng dẫn phát triển, và các quy trình liên quan đến dự án.

## Mục lục

### 1. Tài liệu Kỹ thuật (Engineering Docs)

-   [Kiến trúc tổng quan](./engineering/architecture.md): Mô tả kiến trúc C4, các thành phần chính và luồng hoạt động của hệ thống.
-   [Hướng dẫn phát triển](./engineering/development-guide.md): Hướng dẫn cài đặt môi trường, build, và deploy dự án.
-   [Frontend](./engineering/frontend-guide.md): Chi tiết về cấu trúc dự án Frontend, state management, và coding style.
-   [Backend](./engineering/backend-guide.md): Chi tiết về cấu trúc dự án Backend, các pattern sử dụng, và coding style.
-   [Hướng dẫn API](./engineering/api-reference.md): Tài liệu chi tiết về các endpoint của API theo chuẩn OpenAPI.
-   [Mô hình dữ liệu](./engineering/data-model.md): Sơ đồ ERD, schema database, và các quan hệ.
-   [Kiểm thử & QA](./engineering/testing-guide.md): Hướng dẫn về chiến lược kiểm thử, các loại test, và quy trình QA.
-   [Bảo mật](./engineering/security-guide.md): Các vấn đề bảo mật và phương pháp kiểm soát truy cập.
-   [Hướng dẫn đóng góp](./engineering/contribution-guide.md): Quy tắc và quy trình để đóng góp vào dự án.

### 2. Tài liệu Dự án (Project Docs)

-   [Product Backlog](./project/backlog.md): Danh sách các User Story.
-   [Kế hoạch Sprint](./project/sprints.md): Kế hoạch cho các sprint phát triển.
-   [Kịch bản Kiểm thử](./project/test-cases.md): Các kịch bản kiểm thử chi tiết.
-   [Ghi chú phát hành](./project/release-notes.md): Lịch sử các phiên bản và thay đổi.
-   [Lộ trình Phát triển](./project/roadmap.md): Định hướng phát triển sản phẩm.
-   [Đội ngũ Phát triển](./project/team.md): Thông tin về các thành viên trong đội.

## Các thay đổi gần đây

-   **Xử lý dữ liệu và Chia Chunk**: Triển khai module xử lý và chia nhỏ nội dung từ các tệp PDF/TXT thành các `TextChunk` để chuẩn bị cho việc tạo embeddings và tích hợp chatbot.
-   **Quản lý Hồ sơ Người dùng tập trung:** Tái cấu trúc frontend để quản lý thông tin hồ sơ người dùng tập trung trong `userProfileStore`, giảm sự phụ thuộc của các component UI vào `authStore`.
-   **API Hồ sơ Người dùng hiện tại:** Thêm endpoint backend mới `GET /api/UserProfiles/me` để lấy hồ sơ của người dùng hiện tại một cách an toàn.
-   **Trường Avatar cho Hồ sơ Người dùng:** Bổ sung trường `Avatar` vào thực thể `UserProfile` và cập nhật các chức năng liên quan ở cả backend và frontend.
-   **Xử lý kết quả nhất quán:** Triển khai `Result` wrapper cho `GetCurrentUserProfileQueryHandler` để đảm bảo xử lý kết quả nhất quán.
